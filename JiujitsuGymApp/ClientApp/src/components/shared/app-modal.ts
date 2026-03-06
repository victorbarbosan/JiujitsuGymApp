import { LitElement, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { TemplateResult } from 'lit';

@customElement('app-modal')
export class AppModal extends LitElement {
    createRenderRoot() {
        return this;
    }

    @property({ type: Boolean })
    open: boolean = false;

    @property({ type: String })
    title: string = '';

    @property({ attribute: false })
    content: () => TemplateResult = () => html``;

    private _close() {
        this.dispatchEvent(new CustomEvent('modal-close', { bubbles: true, composed: true }));
    }

    render() {
        if (!this.open) return html``;

        return html`
            <div class="modal d-block" tabindex="-1" style="background:rgba(0,0,0,0.5);">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${this.title}</h5>
                            <button type="button" class="btn-close" @click=${this._close}></button>
                        </div>
                        ${this.content()}
                    </div>
                </div>
            </div>
        `;
    }
}