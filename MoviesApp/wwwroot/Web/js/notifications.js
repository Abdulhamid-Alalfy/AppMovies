/**
 * NotificationManager - Handles real-time notifications via SignalR
 * Consolidates UI logic for the notification bell and toast alerts.
 */
class NotificationManager {
    constructor() {
        // Core elements
        this.notificationIcon = document.getElementById('notificationIcon');
        this.notificationBadge = document.getElementById('notificationBadge');
        this.notificationPanel = document.getElementById('notificationPanel');
        this.notificationList = document.getElementById('notificationList');
        this.markAllBtn = document.getElementById('markAllAsReadBtn');

        this.isPanelOpen = false;
        this.connection = null;

        if (this.notificationIcon) {
            this.init();
        }
    }

    init() {
        this.setupEventListeners();
        this.setupSignalR();
        // Initial count check
        this.updateNotificationCount();
    }

    setupEventListeners() {
        // Toggle Panel
        this.notificationIcon.addEventListener('click', (e) => {
            e.stopPropagation();
            this.togglePanel();
        });

        // Close on outside click
        document.addEventListener('click', (e) => {
            if (this.isPanelOpen && !this.notificationPanel.contains(e.target) && !this.notificationIcon.contains(e.target)) {
                this.closePanel();
            }
        });

        // Mark all as read
        if (this.markAllBtn) {
            this.markAllBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.markAllAsRead();
            });
        }
    }

    setupSignalR() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/notificationHub')
                .withAutomaticReconnect()
                .build();

            this.connection.on('ReceiveNotification', (notification) => {
                this.handleIncomingNotification(notification);
            });

            this.connection.start()
                .then(() => console.log('🔔 SignalR Connected'))
                .catch(err => console.error('❌ SignalR Connection Error: ', err));

        } catch (err) {
            console.error('SignalR initialization failed', err);
        }
    }

    async togglePanel() {
        if (this.isPanelOpen) {
            this.closePanel();
        } else {
            await this.openPanel();
        }
    }

    async openPanel() {
        this.notificationPanel.classList.add('active');
        this.isPanelOpen = true;
        await this.loadNotifications();
    }

    closePanel() {
        this.notificationPanel.classList.remove('active');
        this.isPanelOpen = false;
    }

    async loadNotifications() {
        if (!this.notificationList) return;

        try {
            const response = await fetch('/Notification/GetNotifications');
            const notifications = await response.json();
            this.renderNotificationList(notifications);
        } catch (error) {
            this.notificationList.innerHTML = '<div class="p-4 text-center text-danger">فشل تحميل الإشعارات</div>';
        }
    }

    renderNotificationList(notifications) {
        if (!notifications || notifications.length === 0) {
            this.notificationList.innerHTML = '<div class="p-4 text-center text-muted">لا توجد إشعارات حالياً</div>';
            return;
        }

        this.notificationList.innerHTML = notifications.map(n => this.createNotificationItemHtml(n)).join('');
    }

    createNotificationItemHtml(n) {
        return `
            <div class="notification-item ${n.isRead ? '' : 'unread'}" onclick="window.notificationManager.handleItemClick(${n.id}, '${n.link}')">
                <div class="icon-box">
                    ${this.getIconByType(n.type)}
                </div>
                <div class="notification-content">
                    <h4>${n.title}</h4>
                    <p>${n.message}</p>
                    <small>${this.formatTime(n.createdAt)}</small>
                </div>
                ${!n.isRead ? '<div class="unread-dot"></div>' : ''}
            </div>
        `;
    }

    getIconByType(type) {
        // Support both integer and string (just in case)
        const typeId = parseInt(type);
        const icons = {
            1: '🎬', // NewMovie
            2: '⚠️', // SystemAlert
            3: '📝', // MovieUpdate
            4: '🎁'  // SpecialOffer
        };
        return icons[typeId] || '🔔';
    }

    handleIncomingNotification(n) {
        // 1. Play subtle sound if desired
        // 2. Show Toast
        this.showToast(n);
        
        // 3. Update Badge
        this.updateNotificationCount();

        // 4. If panel is open, prepend to list
        if (this.isPanelOpen) {
            this.loadNotifications(); // Reload for simplicity or prepend
        }
    }

    showToast(n) {
        const toast = document.createElement('div');
        toast.className = 'notification-toast';
        toast.innerHTML = `
            <div class="toast-icon">${this.getIconByType(n.type)}</div>
            <div class="toast-body" style="flex: 1;">
                <h6 style="margin: 0 0 5px; color: #fff; font-weight: 700;">${n.title}</h6>
                <p style="margin: 0; font-size: 0.85rem; color: rgba(255,255,255,0.7);">${n.message}</p>
            </div>
        `;

        document.body.appendChild(toast);
        
        // Animate in
        setTimeout(() => toast.classList.add('show'), 100);

        // Click to open link
        toast.onclick = () => {
            if (n.link) window.location.href = n.link;
        };

        // Auto remove
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 500);
        }, 6000);
    }

    async updateNotificationCount() {
        try {
            const response = await fetch('/Notification/GetNotificationCount');
            const data = await response.json();
            
            if (this.notificationBadge) {
                if (data.count > 0) {
                    this.notificationBadge.textContent = data.count;
                    this.notificationBadge.style.display = 'flex';
                } else {
                    this.notificationBadge.style.display = 'none';
                }
            }
        } catch (err) {
            console.error('Count update failed', err);
        }
    }

    async handleItemClick(id, link) {
        // Mark as read first
        try {
            await fetch('/Notification/MarkAsRead', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: id })
            });
        } catch (e) {}

        if (link && link !== 'null') {
            window.location.href = link;
        } else {
            this.loadNotifications();
            this.updateNotificationCount();
        }
    }

    async markAllAsRead() {
        try {
            const response = await fetch('/Notification/MarkAllAsRead', { method: 'POST' });
            if (response.ok) {
                this.loadNotifications();
                this.updateNotificationCount();
            }
        } catch (e) {}
    }

    formatTime(dateStr) {
        const date = new Date(dateStr);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        
        if (diffMins < 1) return 'الآن';
        if (diffMins < 60) return `منذ ${diffMins} دقيقة`;
        
        const diffHours = Math.floor(diffMs / 3600000);
        if (diffHours < 24) return `منذ ${diffHours} ساعة`;
        
        return date.toLocaleDateString('ar-EG');
    }
}

// Initialize on load
document.addEventListener('DOMContentLoaded', () => {
    window.notificationManager = new NotificationManager();
});