import { LitElement, html, css } from 'lit';

export class ProfileInfo extends LitElement {
    static properties = {
        label: { type: String },
        value: { type: String },
    };

    render() {
        return html`
      <div>
        <span class="label">${this.label}</span>
        <span class="value">${this.value || 'Not provided'}</span>
      </div>
    `;
    }

    static styles = css`
    .label {
      display: block;
      font-size: var(--font-size-xs);
      color: var(--color-text-secondary);
      font-weight: bold;
      text-transform: uppercase;
    }

    .value {
      font-size: var(--font-size-md);
      color: var(--color-text-primary);
    }
  `;
}

customElements.define('profile-info', ProfileInfo);
