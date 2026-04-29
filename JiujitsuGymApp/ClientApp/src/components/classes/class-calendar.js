import { LitElement, html } from 'lit';

const DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
const SHORT_DAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

function startOfWeek(date) {
    const d = new Date(date);
    d.setUTCDate(d.getUTCDate() - d.getUTCDay());
    d.setUTCHours(0, 0, 0, 0);
    return d;
}

function formatDate(date) {
    return date.toISOString().slice(0, 10);
}

function formatLocalDate(date) {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
}

function formatTime(isoString) {
    return new Date(isoString).toLocaleTimeString('en-US', {
        hour: 'numeric', minute: '2-digit', hour12: true, timeZone: 'UTC'
    });
}

class ClassCalendar extends LitElement {
    static properties = {
        weekStart:       { type: String, attribute: 'week-start' },
        initialClasses:  { type: Array,  attribute: 'initial-classes' },
        currentWeek:     { state: true },
        classes:         { state: true },
        isLoading:       { state: true },
    };

    createRenderRoot() { return this; }

    constructor() {
        super();
        this.weekStart = '';
        this.initialClasses = [];
        this.currentWeek = null;
        this.classes = [];
        this.isLoading = false;
    }

    connectedCallback() {
        super.connectedCallback();
        this.currentWeek = this.weekStart
            ? startOfWeek(this.weekStart)
            : startOfWeek(new Date());
        this.classes = [...this.initialClasses];
    }

    get _weekDays() {
        return Array.from({ length: 7 }, (_, i) => {
            const d = new Date(this.currentWeek);
            d.setUTCDate(d.getUTCDate() + i);
            return d;
        });
    }

    get _isCurrentWeek() {
        return formatDate(this.currentWeek) === formatDate(startOfWeek(new Date()));
    }

    async _goToToday() {
        this.currentWeek = startOfWeek(new Date());
        await this._loadWeek();
    }

    async _navigate(offsetWeeks) {
        const next = new Date(this.currentWeek);
        next.setUTCDate(next.getUTCDate() + offsetWeeks * 7);
        this.currentWeek = next;
        await this._loadWeek();
    }

    async _loadWeek() {
        this.isLoading = true;
        const from = formatDate(this.currentWeek);
        const to = formatDate(new Date(this.currentWeek.getTime() + 7 * 86400000));
        try {
            const res = await fetch(`/Classes/GetClasses?from=${from}&to=${to}`);
            this.classes = await res.json();
        } catch {
            console.error('Failed to load classes');
        } finally {
            this.isLoading = false;
        }
    }

    async _handleCheckIn(classId) {
        const isCheckedIn = this.classes.find(c => c.id === classId)?.checkedIn ?? false;
        const url = `/Classes/${isCheckedIn ? 'UndoCheckIn' : 'CheckIn'}/${classId}`;
        const method = isCheckedIn ? 'DELETE' : 'POST';

        try {
            const res = await fetch(url, {
                method,
                headers: { 'RequestVerificationToken': getAntiForgeryToken() }
            });
            if (res.ok) {
                this.classes = this.classes.map(c =>
                    c.id === classId
                        ? {
                            ...c,
                            checkedIn: !isCheckedIn,
                            attendanceCount: c.attendanceCount + (isCheckedIn ? -1 : 1)
                          }
                        : c
                );
            }
        } catch {
            console.error('Check-in toggle failed');
        }
    }

    _renderDayColumn(day, index) {
        const dateStr = formatDate(day);
        const isToday = dateStr === formatLocalDate(new Date());
        const dayClasses = this.classes.filter(c => c.dateTime.slice(0, 10) === dateStr);

        return html`
        <div class="col">
            <div class="text-center mb-2">
                <div class="small text-muted">${SHORT_DAYS[index]}</div>
                <div class="fw-bold fs-5 ${isToday ? 'text-primary' : ''}">
                    ${day.getUTCDate()}
                    ${isToday ? html`<span class="badge bg-primary ms-1" style="font-size:.55rem;vertical-align:middle">Today</span>` : ''}
                </div>
            </div>
            <div class="d-flex flex-column gap-2">
                ${dayClasses.length === 0
                    ? html`<div class="text-center text-muted small py-3">empty</div>`
                    : dayClasses.map(c => this._renderClassCard(c))}
            </div>
        </div>`;
    }

    _renderClassCard(c) {
        return html`
        <div class="card shadow-sm border-0 ${c.checkedIn ? 'border-start border-success border-3' : ''}">
            <div class="card-body p-2">
                <div class="fw-semibold small">${formatTime(c.dateTime)}</div>
                <div class="small text-muted">${c.location}</div>
                <div class="small text-muted">
                    <i class="fas fa-user me-1"></i>${c.teacherName}
                </div>
                <div class="d-flex justify-content-between align-items-center mt-1">
                    <span class="small text-muted">
                        <i class="fas fa-users me-1"></i>${c.attendanceCount}
                    </span>
                    <button
                        class="btn btn-sm py-0 px-2 ${c.checkedIn ? 'btn-success' : 'btn-outline-primary'}"
                        @click=${() => this._handleCheckIn(c.id)}>
                        ${c.checkedIn
                            ? html`<i class="fas fa-check me-1"></i>Checked in`
                            : 'Check in'}
                    </button>
                </div>
            </div>
        </div>`;
    }

    render() {
        const days = this._weekDays;
        const weekLabel = `${days[0].toLocaleDateString('en-US', { month: 'short', day: 'numeric', timeZone: 'UTC' })} to ${days[6].toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric', timeZone: 'UTC' })}`;

        return html`
        <div class="d-flex justify-content-between align-items-center mb-4">
            <h1 class="h4 mb-0"><i class="fas fa-calendar-alt me-2"></i>Classes</h1>
            <div class="d-flex align-items-center gap-2">
                ${!this._isCurrentWeek ? html`
                    <button class="btn btn-outline-primary btn-sm" @click=${() => this._goToToday()}>
                        <i class="fas fa-calendar-day me-1"></i>Today
                    </button>` : ''}
                <button class="btn btn-outline-secondary btn-sm" @click=${() => this._navigate(-1)}>
                    <i class="fas fa-chevron-left"></i>
                </button>
                <span class="fw-semibold">${weekLabel}</span>
                <button class="btn btn-outline-secondary btn-sm" @click=${() => this._navigate(1)}>
                    <i class="fas fa-chevron-right"></i>
                </button>
            </div>
        </div>

        ${this.isLoading ? html`
            <div class="text-center py-5">
                <div class="spinner-border text-primary" role="status"></div>
            </div>` : html`
            <div class="row row-cols-7 g-2">
                ${days.map((d, i) => this._renderDayColumn(d, i))}
            </div>`}`;
    }
}

customElements.define('class-calendar', ClassCalendar);