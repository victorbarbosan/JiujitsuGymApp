import { LitElement, html, css } from "lit";

export class SearchBar extends LitElement {
  createRenderRoot() {
    return this;
  }

  static properties = {
    placeholder: { type: String },
    debounceMs: { type: Number },
    value: { type: String },
  };

  constructor() {
    super();
    this.placeholder = "Search...";
    this.debounceMs = 300;
    this.value = "";
    this._debounceTimer = null;
  }

  handleInput(e) {
    this.value = e.target.value;
    clearTimeout(this._debounceTimer);
    this._debounceTimer = setTimeout(() => {
      this.dispatchEvent(
        new CustomEvent("search-change", {
          detail: { query: this.value.trim() },
          bubbles: true,
          composed: true,
        }),
      );
    }, this.debounceMs);
  }

  handleClear() {
    this.value = "";
    clearTimeout(this._debounceTimer);
    this.dispatchEvent(
      new CustomEvent("search-change", {
        detail: { query: "" },
        bubbles: true,
        composed: true,
      }),
    );
  }

  render() {
    return html`
      <div class="input-group">
        <span class="input-group-text">
          <i class="fas fa-search"></i>
        </span>
        <input
          type="search"
          class="form-control"
          placeholder=${this.placeholder}
          .value=${this.value}
          @input=${this.handleInput}
        />        
      </div>
    `;
  }
}

customElements.define("search-bar", SearchBar);