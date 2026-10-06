# مواصفة ViewModel لتفعيل/تعطيل حساب مستخدم (Admin)

## الغرض
وثيقة صغيرة تشرح الـ ViewModel وواجهات الطلب/الاستجابة (API contract)، وقواعد التحقق، ومتطلبات قاعدة البيانات، واقتراحات للواجهة (Razor) وسلوك الأمان. ستقوم أنت بنقل هذه المواصفة إلى كود المشروع.

## اسم الكلاس المقترح
`UserActivationViewModel`

## الحقول (عرض للعرض والتحكم)
- `UserId` : `Guid` أو `int` — معرف المستخدم (مطلوب عند الإرسال).
- `UserName` : `string` — اسم المستخدم (عرض فقط).
- `Email` : `string` — البريد الإلكتروني (عرض فقط).
- `IsActive` : `bool` — حالة الحساب الحالية.
- `CreatedAt` : `DateTime?` — تاريخ الإنشاء (عرض فقط).
- `LastLoginAt` : `DateTime?` — آخر تسجيل دخول (عرض فقط).
- `ChangeReason` : `string?` — سبب التفعيل/التعطيل (اختياري، نص من 0-500 حرف).

> ملاحظة: للحركات البسيطة على الـ UI في صفحة الإدارة يكفي حقلا `UserId` و`IsActive` و`ChangeReason` في الطلب.

## DTOs (API)
- طلب (Request): `UserActivationRequest`
  - `UserId`: `Guid` | `int` (مطلوب)
  - `SetActive`: `bool` (مطلوب) — القيمة المراد ضبطها
  - `Reason`: `string?` (اختياري)

- استجابة (Response): `UserActivationResponse`
  - `Success`: `bool`
  - `Message`: `string?`
  - `UpdatedUser`: `UserActivationViewModel?` (اختياري، لإظهار النتيجة)

## مسار الـ API المقترح
- `POST /api/admin/users/{id}/activation`
  - Authorization: فقط للمستخدمين ذوي الدور `Admin` أو صلاحية إدارة الحسابات.
  - Body: JSON يطابق `UserActivationRequest` (يمكن تجاهل `UserId` بالـ body لأنّه في المسار).
  - حالات الاستجابة:
    - 200: { Success: true, UpdatedUser: {...} }
    - 400: خطأ في التحقق (مثال: Reason طويل جداً)
    - 401/403: غير مفوَّض
    - 404: مستخدم غير موجود

## قواعد التحقق (Validation)
- `UserId` يجب أن يكون موجودًا وصالحًا.
- `Reason` طول النص ≤ 500 حرف.
- لا تسمح بتعطيل آخر حساب مسؤول إداري إذا لم يكن هناك مسؤول آخر (اختياري حسب رغبتكم).
- يمنع تعطيل حساب المدير الذي يقوم بالعملية (لا تسمح للنفس أن تعطل نفسها) — تحقق من `currentAdminId != targetUserId`.

## تغييرات قاعدة البيانات المقترحة
- حقل موجود/مطلوب: أضف عمود `IsActive` في جدول المستخدمين (`AppUsers` أو ما شابهه) إن لم يكن موجودًا:

SQL مقترح (MSSQL):

```sql
ALTER TABLE AppUsers
ADD IsActive BIT NOT NULL CONSTRAINT DF_AppUsers_IsActive DEFAULT 1;

-- تهيئة للبيانات القديمة (اختياري)
UPDATE AppUsers SET IsActive = 1 WHERE IsActive IS NULL;
```

أو إن تستخدم EF Core: أضف `public bool IsActive { get; set; } = true;` في كلاس `AppUser` ثم قم بإنشاء Migration.

## تسجيل النشاط (Audit)
- عند كل تغيير: سجّل سجلًا في جدول `AuditLogs` أو `Notification` يحتوي على:
  - `AdminId`، `TargetUserId`، `Action` (Activate/Deactivate)، `Reason`، `Timestamp`.
- هذا يسهل التراجع أو المراجعة لاحقًا.

## أذونات وأمن
- واجهة API: الزِم صفة `[Authorize(Roles = "Admin")]` أو نظام صلاحيات مكافئ.
- تحقق على مستوى الخدمة أيضاً (مضاعفة مستوى الحماية) وليس فقط في الـ controller.
- منع CSRF في صفحات الـ MVC/Razor (استخدم الـ anti-forgery tokens عند تنفيذ الطلبات من صفحات الـ admin).

## اقتراح واجهة المستخدم (Admin UI)
- صفحة إدارة المستخدمين: جدول يعرض `UserName`, `Email`, `IsActive`, زر/تبديل (toggle) لتغيير الحالة.
- عند النقر على زر التبديل: إظهار نافذة صغيرة (modal) تطلب تأكيد وسبب اختياري.
- بعد التأكيد: إرسال `POST /api/admin/users/{id}/activation`، ثم تحديث الصف/الصفحة بناءً على `UpdatedUser`.

مثال مختصر لزر في Razor (اقتراح، ليس كود نهائي):

- عرض زر تبديل بجانب كل مستخدم: "Activate"/"Deactivate"، يفتح modal.
- داخل الـ modal: حقل نصي `Reason` وزران: Confirm / Cancel.

## اختبارات مقترحة
- اختبار الوحدة (Unit):
  - تفعيل/تعطيل مستخدم ناجح يُحدث `IsActive` ويُسجل Audit.
  - محاولة من غير Admin => يفشل (403).
  - محاولة تعطيل نفس الـ Admin الحالي => تُرفض.
  - طلب مع معرف غير صالح => 404.
- اختبار تكاملي (Integration): التأكد من أن الطلب يغيّر الحقل في DB ويُرجع الـ ViewModel المحدث.

## حالات حديّة واعتبارات إضافية
- إذا كنتم تستخدمون نظام تسجيل الدخول اعتمادًا على `IsActive`، تأكدوا من فحص الحقل أثناء المصادقة: منع المستخدم المعطل من تسجيل الدخول.
- إرسال إشعار للمستخدم عند التعطيل/التفعيل (بريد/إشعار في التطبيق).
- إضافة تاريخ `DisabledAt` و`DisabledBy` إذا أردتم سجلًا أكثر تفصيلاً.

## مثال سريع للـ ViewModel (على سبيل التوضيح — أنت ستطبّقه بنفسك):

```
public class UserActivationViewModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? ChangeReason { get; set; }
}
```

## أين أضع الملف
وضعت هذه الوثيقة هنا: [MoviesApp/Docs/UserActivationViewModel.md](MoviesApp/Docs/UserActivationViewModel.md)

---
إذا تريد، أقدر أقدم: نموذجٍ كاملٍ للـ controller action، أو ملف Migration جاهز لـ EF Core، أو مقتطفات Razor/JS للـ modal وAJAX. أيّ واحد تريد أطبّقه next؟