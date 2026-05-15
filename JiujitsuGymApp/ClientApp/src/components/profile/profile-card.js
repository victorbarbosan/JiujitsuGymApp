import { LitElement, html, css } from "lit";
import { getBeltConfig, formatDate } from "./profile-utils.js";

import "./profile-header.js";
import "./profile-info.js";
import "./profile-actions.js";
import "./profile-progression.js";

class ProfileCard extends LitElement {
  static properties = {
    editMode: { type: Boolean },
    firstName: { type: String, attribute: "first-name" },
    lastName: { type: String, attribute: "last-name" },
    email: { type: String },
    phoneNumber: { type: String, attribute: "phone-number" },
    belt: { type: String },
    memberSince: { type: String, attribute: "member-since" },
    lastLogin: { type: String, attribute: "last-login" },
    attendanceCount: { type: Number, attribute: "attendance-count" },
  };

  constructor() {
    super();
    this.editMode = false;
    this.firstName = "";
    this.lastName = "";
    this.email = "";
    this.phoneNumber = "";
    this.belt = "";
    this.memberSince = "";
    this.lastLogin = "";
    this.attendanceCount = 0;
  }

  #enableEdit() {
    this.editMode = true;
  }

  render() {
    const { main, text, accent } = getBeltConfig(this.belt);

    return html`
      <profile-progression
        .belt=${this.belt}
        .attendanceCount=${this.attendanceCount}
      ></profile-progression>
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
            <profile-info
              label="Phone"
              .value=${this.phoneNumber}
            ></profile-info>
            <profile-info
              label="Member Since"
              .value=${formatDate(this.memberSince)}
            ></profile-info>
            <profile-info
              label="Last Login"
              .value=${formatDate(this.lastLogin)}
            ></profile-info>
          </div>

          <profile-actions
            @edit-profile=${() => this.#enableEdit()}
          ></profile-actions>
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

    profile-progression {
      display: block;
      margin-top: var(--space-4);
    }
  `;
}

customElements.define("profile-card", ProfileCard);
