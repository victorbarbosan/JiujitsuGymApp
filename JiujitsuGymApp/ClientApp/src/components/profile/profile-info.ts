import { LitElement, html, css } from 'lit';
import { property } from 'lit/decorators.js';

export class ProfileInfo extends LitElement {
  @property({ type: String, attribute: 'label' })
  label = '';

  @property({ type: String, attribute: 'value' })
  value = '';

  private get displayValue() {
    return this.value || 'Not provided';
  }

  render() {
    return html`
      <div>
        <span class="label">${this.label}</span>
        <span class="value">${this.displayValue}</span>
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
