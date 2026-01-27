import { LitElement, html, css } from 'lit';

export class ProfileActions extends LitElement {
    render() {
        return html`
      <div class="actions">
        <button @click=${this.#edit}>Edit Profile</button>
        <button class="outline" @click=${this.#password}>
          Change Password
        </button>
      </div>
    `;
    }

    #edit() {
        this.dispatchEvent(new CustomEvent('edit-profile', { 
            bubbles: true,
            composed: true 
        }));
    }

    #password() {
        this.dispatchEvent(new CustomEvent('change-password', { 
            bubbles: true,
            composed: true 
        }));
    }

    static styles = css`
    .actions {
      display: flex;
      gap: var(--space-3);
      margin-top: var(--space-4);
    }

    button {
      flex: 1;
      padding: var(--space-3);
      border-radius: var(--radius-md);
      font-weight: 600;
      cursor: pointer;
      border: none;
      background: var(--color-brand-primary);
      color: var(--color-text-inverse);
    }

    .outline {
      background: transparent;
      color: var(--color-brand-primary);
      border: 2px solid var(--color-brand-primary);
    }
  `;
}

customElements.define('profile-actions', ProfileActions);
