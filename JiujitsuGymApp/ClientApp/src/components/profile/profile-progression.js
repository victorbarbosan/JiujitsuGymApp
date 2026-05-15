import { LitElement, html, css } from "lit";
import { getBeltProgression, getBeltConfig } from "./profile-utils.js";

class ProfileProgression extends LitElement {
  static properties = {
    belt: { type: String },
    attendanceCount: { type: Number, attribute: "attendance-count" },
  };

  constructor() {
    super();
    this.belt = "";
    this.attendanceCount = 0;
  }

  render() {
    const progression = getBeltProgression(this.belt);
    const { main } = getBeltConfig(this.belt);

    if (!progression) {
      return html`
        <div class="progression">
          <span class="progression-label"
            >🥋 Black Belt — Maximum rank achieved</span
          >
        </div>
      `;
    }

    const { nextBelt, goal } = progression;
    const count = Math.min(this.attendanceCount, goal);
    const percent = Math.round((count / goal) * 100);
    const remaining = goal - count;

    return html`
      <div class="progression" style="--belt-color:${main}">
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
            ? html`<span
                >${remaining} class${remaining === 1 ? "" : "es"}
                remaining</span
              >`
            : html`<span class="progression-ready"
                >🎉 Ready for promotion!</span
              >`}
          <span>${percent}%</span>
        </div>
      </div>
    `;
  }

  static styles = css`
    :host {
      display: block;
      margin-top: var(--space-4);
      margin-bottom: var(--space-4);
    }

    .progression {
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
      margin-bottom: var(--space-2);
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
      border: 1px solid rgba(0, 0, 0, 0.1);
      transition: width 0.6s ease;
    }

    .progression-ready {
      color: #2e7d32;
      font-weight: 600;
    }
  `;
}

customElements.define("profile-progression", ProfileProgression);
