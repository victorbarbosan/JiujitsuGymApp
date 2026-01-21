import { LitElement, html, css } from 'lit';
import { property } from 'lit/decorators.js';
import { getBeltConfig, formatDate } from './profile-utils';

import './profile-header';
import './profile-info';
import './profile-actions';

export class ProfileCard extends LitElement {
  @property({ type: String, attribute: 'first-name' })
  firstName = '';

  @property({ type: String, attribute: 'last-name' })
  lastName = '';

  @property({ type: String })
  email = '';

  @property({ type: String, attribute: 'phone-number' })
  phoneNumber = '';

  @property({ type: String })
  belt = '';

  @property({ type: String, attribute: 'member-since' })
  memberSince = '';

  @property({ type: String, attribute: 'last-login' })
  lastLogin = '';

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
            <profile-info label="Member Since"
              .value=${formatDate(this.memberSince)}>
            </profile-info>
            <profile-info label="Last Login"
              .value=${formatDate(this.lastLogin)}>
            </profile-info>
          </div>

          <profile-actions></profile-actions>
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
  `;
}

customElements.define("profile-card", ProfileCard);
