import { LitElement, html, css } from 'lit';

export class ProfileHeader extends LitElement {
    static properties = {
        firstName: { type: String },
        lastName: { type: String },
        belt: { type: String },
    };

    get initial() {
        return this.firstName ? this.firstName[0].toUpperCase() : '?';
    }

    render() {
        return html`
      <div class="profile-header">
        <div class="profile-avatar">${this.initial}</div>
        <h2 class="profile-name">
          ${this.firstName} ${this.lastName}
        </h2>
        ${this.belt
                ? html`<div class="profile-belt">${this.belt} Belt</div>`
                : null}
      </div>
    `;
    }

    static styles = css`
    .profile-header {
      padding: var(--space-5) var(--space-4);
      text-align: center;
      color: var(--belt-text);
      background: linear-gradient(
        135deg,
        var(--belt-color) 0%,
        #444 150%
      );
    }

    .profile-avatar {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      background: var(--color-surface);
      margin: 0 auto var(--space-3);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 32px;
      font-weight: bold;
      color: var(--avatar-accent);
    }

    .profile-name {
      font-size: var(--font-size-lg);
      margin-bottom: var(--space-2);
    }

    .profile-belt {
      display: inline-block;
      padding: var(--space-1) var(--space-3);
      border-radius: var(--radius-pill);
      background: rgba(0,0,0,.1);
      font-size: var(--font-size-sm);
      font-weight: bold;
      text-transform: uppercase;
    }
  `;
}

customElements.define('profile-header', ProfileHeader);
