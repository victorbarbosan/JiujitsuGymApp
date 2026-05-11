import { LitElement, html, css } from 'lit';
import { getBeltConfig, formatDate, getBeltProgression } from './profile-utils.js';

import './profile-header.js';
import './profile-info.js';
import './profile-actions.js';

class ProfileCard extends LitElement {
    static properties = {
        editMode:       { type: Boolean },
        firstName:      { type: String, attribute: 'first-name' },
        lastName:       { type: String, attribute: 'last-name' },
        email:          { type: String },
        phoneNumber:    { type: String, attribute: 'phone-number' },
        belt:           { type: String },
        memberSince:    { type: String, attribute: 'member-since' },
        lastLogin:      { type: String, attribute: 'last-login' },
        attendanceCount:{ type: Number, attribute: 'attendance-count' },
    };

    constructor() {
        super();
        this.editMode        = false;
        this.firstName       = '';
        this.lastName        = '';
        this.email           = '';
        this.phoneNumber     = '';
        this.belt            = '';
        this.memberSince     = '';
        this.lastLogin       = '';
        this.attendanceCount = 0;
    }

    #enableEdit() {
        this.editMode = true;
    }

    #renderProgressBar() {
        const progression = getBeltProgression(this.belt);
        if (!progression) {
            return html`
                <div class="progression">
                    <span class="progression-label">🥋 Black Belt — Maximum rank achieved</span>
                </div>
            `;
        }

        const { nextBelt, goal } = progression;
        const count      = Math.min(this.attendanceCount, goal);
        const percent    = Math.round((count / goal) * 100);
        const remaining  = goal - count;

        return html`
            <div class="progression">
                <div class="progression-header">
                    <span class="progression-label">
                        Progress to <strong>${nextBelt} Belt</strong>
                    </span>
                    <span class="progression-count">${count} / ${goal} classes</span>
                </div>
                <div class="progress-track">
                    <div class="progress-fill" style="width:${percent}%"></div>
                </div>
                <div class="progression-footer">
                    ${remaining > 0
                        ? html`<span>${remaining} class${remaining === 1 ? '' : 'es'} remaining</span>`
                        : html`<span class="progression-ready">🎉 Ready for promotion!</span>`}
                    <span>${percent}%</span>
                </div>
            </div>
        `;
    }

    render() {
        const { main, text, accent } = getBeltConfig(this.belt);

        return html`
            <div
                class="card"
                style="
                    --belt-color:${main};
                    --belt-text:${text};
                    --avatar-accent:${accent};
                "
            >
                <profile-header
                    .firstName=${this.firstName}
                    .lastName=${this.lastName}
                    .belt=${this.belt}
                ></profile-header>

                <div class="body">
                    <div class="grid">
                        <profile-info label="Email" .value=${this.email}></profile-info>
                        <profile-info label="Phone" .value=${this.phoneNumber}></profile-info>
                        <profile-info label="Member Since" .value=${formatDate(this.memberSince)}></profile-info>
                        <profile-info label="Last Login"   .value=${formatDate(this.lastLogin)}></profile-info>
                    </div>

                    ${this.#renderProgressBar()}

                    <profile-actions @edit-profile=${() => this.#enableEdit()}></profile-actions>
                </div>
            </div>
        `;
    }

    static styles = css`
        .card {
            background: var(--color-surface);
            border-radius: var(--radius-lg);
            box-shadow: var(--shadow-md);
            border: 1px solid var(--color-border);
            overflow: hidden;
        }

        .body {
            padding: var(--space-4);
        }

        .grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: var(--space-4);
        }

        /* ── Progression bar ── */
        .progression {
            margin-top: var(--space-4);
            padding: var(--space-3);
            background: var(--color-surface-alt, #f5f5f5);
            border-radius: var(--radius-md);
            border: 1px solid var(--color-border);
        }

        .progression-header,
        .progression-footer {
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-size: var(--font-size-sm);
            color: var(--color-text-secondary);
            margin-bottom: var(--space-2);
        }

        .progression-footer {
            margin-top: var(--space-2);
            margin-bottom: 0;
        }

        .progression-label {
            font-size: var(--font-size-sm);
            color: var(--color-text-secondary);
        }

        .progression-count {
            font-weight: 600;
            color: var(--color-text-primary);
        }

        .progress-track {
            height: 10px;
            border-radius: var(--radius-pill);
            background: var(--color-border);
            overflow: hidden;
        }

        .progress-fill {
            height: 100%;
            border-radius: var(--radius-pill);
            background: var(--belt-color, #667eea);
            border: 1px solid rgba(0,0,0,.1);
            transition: width 0.6s ease;
        }

        .progression-ready {
            color: #2e7d32;
            font-weight: 600;
        }
    `;
}

customElements.define('profile-card', ProfileCard);