// تحديث اسم الملف عند اختيار صورة الملف الشخصي
document.getElementById('UserImage').addEventListener('change', function (e) {
    var fileName = e.target.files[0] ? e.target.files[0].name : '';
    document.getElementById('fileNamePreview').innerText = fileName;
});