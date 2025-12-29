var pt=Object.defineProperty;var ft=(r,t,e)=>t in r?pt(r,t,{enumerable:!0,configurable:!0,writable:!0,value:e}):r[t]=e;var E=(r,t,e)=>ft(r,typeof t!="symbol"?t+"":t,e);/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const M=globalThis,q=M.ShadowRoot&&(M.ShadyCSS===void 0||M.ShadyCSS.nativeShadow)&&"adoptedStyleSheets"in Document.prototype&&"replace"in CSSStyleSheet.prototype,G=Symbol(),J=new WeakMap;let nt=class{constructor(t,e,s){if(this._$cssResult$=!0,s!==G)throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");this.cssText=t,this.t=e}get styleSheet(){let t=this.o;const e=this.t;if(q&&t===void 0){const s=e!==void 0&&e.length===1;s&&(t=J.get(e)),t===void 0&&((this.o=t=new CSSStyleSheet).replaceSync(this.cssText),s&&J.set(e,t))}return t}toString(){return this.cssText}};const ut=r=>new nt(typeof r=="string"?r:r+"",void 0,G),ot=(r,...t)=>{const e=r.length===1?r[0]:t.reduce(((s,i,o)=>s+(n=>{if(n._$cssResult$===!0)return n.cssText;if(typeof n=="number")return n;throw Error("Value passed to 'css' function must be a 'css' function result: "+n+". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.")})(i)+r[o+1]),r[0]);return new nt(e,r,G)},$t=(r,t)=>{if(q)r.adoptedStyleSheets=t.map((e=>e instanceof CSSStyleSheet?e:e.styleSheet));else for(const e of t){const s=document.createElement("style"),i=M.litNonce;i!==void 0&&s.setAttribute("nonce",i),s.textContent=e.cssText,r.appendChild(s)}},K=q?r=>r:r=>r instanceof CSSStyleSheet?(t=>{let e="";for(const s of t.cssRules)e+=s.cssText;return ut(e)})(r):r;/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const{is:bt,defineProperty:gt,getOwnPropertyDescriptor:mt,getOwnPropertyNames:_t,getOwnPropertySymbols:yt,getPrototypeOf:vt}=Object,b=globalThis,Y=b.trustedTypes,At=Y?Y.emptyScript:"",z=b.reactiveElementPolyfillSupport,C=(r,t)=>r,I={toAttribute(r,t){switch(t){case Boolean:r=r?At:null;break;case Object:case Array:r=r==null?r:JSON.stringify(r)}return r},fromAttribute(r,t){let e=r;switch(t){case Boolean:e=r!==null;break;case Number:e=r===null?null:Number(r);break;case Object:case Array:try{e=JSON.parse(r)}catch{e=null}}return e}},at=(r,t)=>!bt(r,t),Z={attribute:!0,type:String,converter:I,reflect:!1,useDefault:!1,hasChanged:at};Symbol.metadata??(Symbol.metadata=Symbol("metadata")),b.litPropertyMetadata??(b.litPropertyMetadata=new WeakMap);let v=class extends HTMLElement{static addInitializer(t){this._$Ei(),(this.l??(this.l=[])).push(t)}static get observedAttributes(){return this.finalize(),this._$Eh&&[...this._$Eh.keys()]}static createProperty(t,e=Z){if(e.state&&(e.attribute=!1),this._$Ei(),this.prototype.hasOwnProperty(t)&&((e=Object.create(e)).wrapped=!0),this.elementProperties.set(t,e),!e.noAccessor){const s=Symbol(),i=this.getPropertyDescriptor(t,s,e);i!==void 0&&gt(this.prototype,t,i)}}static getPropertyDescriptor(t,e,s){const{get:i,set:o}=mt(this.prototype,t)??{get(){return this[e]},set(n){this[e]=n}};return{get:i,set(n){const l=i==null?void 0:i.call(this);o==null||o.call(this,n),this.requestUpdate(t,l,s)},configurable:!0,enumerable:!0}}static getPropertyOptions(t){return this.elementProperties.get(t)??Z}static _$Ei(){if(this.hasOwnProperty(C("elementProperties")))return;const t=vt(this);t.finalize(),t.l!==void 0&&(this.l=[...t.l]),this.elementProperties=new Map(t.elementProperties)}static finalize(){if(this.hasOwnProperty(C("finalized")))return;if(this.finalized=!0,this._$Ei(),this.hasOwnProperty(C("properties"))){const e=this.properties,s=[..._t(e),...yt(e)];for(const i of s)this.createProperty(i,e[i])}const t=this[Symbol.metadata];if(t!==null){const e=litPropertyMetadata.get(t);if(e!==void 0)for(const[s,i]of e)this.elementProperties.set(s,i)}this._$Eh=new Map;for(const[e,s]of this.elementProperties){const i=this._$Eu(e,s);i!==void 0&&this._$Eh.set(i,e)}this.elementStyles=this.finalizeStyles(this.styles)}static finalizeStyles(t){const e=[];if(Array.isArray(t)){const s=new Set(t.flat(1/0).reverse());for(const i of s)e.unshift(K(i))}else t!==void 0&&e.push(K(t));return e}static _$Eu(t,e){const s=e.attribute;return s===!1?void 0:typeof s=="string"?s:typeof t=="string"?t.toLowerCase():void 0}constructor(){super(),this._$Ep=void 0,this.isUpdatePending=!1,this.hasUpdated=!1,this._$Em=null,this._$Ev()}_$Ev(){var t;this._$ES=new Promise((e=>this.enableUpdating=e)),this._$AL=new Map,this._$E_(),this.requestUpdate(),(t=this.constructor.l)==null||t.forEach((e=>e(this)))}addController(t){var e;(this._$EO??(this._$EO=new Set)).add(t),this.renderRoot!==void 0&&this.isConnected&&((e=t.hostConnected)==null||e.call(t))}removeController(t){var e;(e=this._$EO)==null||e.delete(t)}_$E_(){const t=new Map,e=this.constructor.elementProperties;for(const s of e.keys())this.hasOwnProperty(s)&&(t.set(s,this[s]),delete this[s]);t.size>0&&(this._$Ep=t)}createRenderRoot(){const t=this.shadowRoot??this.attachShadow(this.constructor.shadowRootOptions);return $t(t,this.constructor.elementStyles),t}connectedCallback(){var t;this.renderRoot??(this.renderRoot=this.createRenderRoot()),this.enableUpdating(!0),(t=this._$EO)==null||t.forEach((e=>{var s;return(s=e.hostConnected)==null?void 0:s.call(e)}))}enableUpdating(t){}disconnectedCallback(){var t;(t=this._$EO)==null||t.forEach((e=>{var s;return(s=e.hostDisconnected)==null?void 0:s.call(e)}))}attributeChangedCallback(t,e,s){this._$AK(t,s)}_$ET(t,e){var o;const s=this.constructor.elementProperties.get(t),i=this.constructor._$Eu(t,s);if(i!==void 0&&s.reflect===!0){const n=(((o=s.converter)==null?void 0:o.toAttribute)!==void 0?s.converter:I).toAttribute(e,s.type);this._$Em=t,n==null?this.removeAttribute(i):this.setAttribute(i,n),this._$Em=null}}_$AK(t,e){var o,n;const s=this.constructor,i=s._$Eh.get(t);if(i!==void 0&&this._$Em!==i){const l=s.getPropertyOptions(i),a=typeof l.converter=="function"?{fromAttribute:l.converter}:((o=l.converter)==null?void 0:o.fromAttribute)!==void 0?l.converter:I;this._$Em=i;const c=a.fromAttribute(e,l.type);this[i]=c??((n=this._$Ej)==null?void 0:n.get(i))??c,this._$Em=null}}requestUpdate(t,e,s){var i;if(t!==void 0){const o=this.constructor,n=this[t];if(s??(s=o.getPropertyOptions(t)),!((s.hasChanged??at)(n,e)||s.useDefault&&s.reflect&&n===((i=this._$Ej)==null?void 0:i.get(t))&&!this.hasAttribute(o._$Eu(t,s))))return;this.C(t,e,s)}this.isUpdatePending===!1&&(this._$ES=this._$EP())}C(t,e,{useDefault:s,reflect:i,wrapped:o},n){s&&!(this._$Ej??(this._$Ej=new Map)).has(t)&&(this._$Ej.set(t,n??e??this[t]),o!==!0||n!==void 0)||(this._$AL.has(t)||(this.hasUpdated||s||(e=void 0),this._$AL.set(t,e)),i===!0&&this._$Em!==t&&(this._$Eq??(this._$Eq=new Set)).add(t))}async _$EP(){this.isUpdatePending=!0;try{await this._$ES}catch(e){Promise.reject(e)}const t=this.scheduleUpdate();return t!=null&&await t,!this.isUpdatePending}scheduleUpdate(){return this.performUpdate()}performUpdate(){var s;if(!this.isUpdatePending)return;if(!this.hasUpdated){if(this.renderRoot??(this.renderRoot=this.createRenderRoot()),this._$Ep){for(const[o,n]of this._$Ep)this[o]=n;this._$Ep=void 0}const i=this.constructor.elementProperties;if(i.size>0)for(const[o,n]of i){const{wrapped:l}=n,a=this[o];l!==!0||this._$AL.has(o)||a===void 0||this.C(o,void 0,n,a)}}let t=!1;const e=this._$AL;try{t=this.shouldUpdate(e),t?(this.willUpdate(e),(s=this._$EO)==null||s.forEach((i=>{var o;return(o=i.hostUpdate)==null?void 0:o.call(i)})),this.update(e)):this._$EM()}catch(i){throw t=!1,this._$EM(),i}t&&this._$AE(e)}willUpdate(t){}_$AE(t){var e;(e=this._$EO)==null||e.forEach((s=>{var i;return(i=s.hostUpdated)==null?void 0:i.call(s)})),this.hasUpdated||(this.hasUpdated=!0,this.firstUpdated(t)),this.updated(t)}_$EM(){this._$AL=new Map,this.isUpdatePending=!1}get updateComplete(){return this.getUpdateComplete()}getUpdateComplete(){return this._$ES}shouldUpdate(t){return!0}update(t){this._$Eq&&(this._$Eq=this._$Eq.forEach((e=>this._$ET(e,this[e])))),this._$EM()}updated(t){}firstUpdated(t){}};v.elementStyles=[],v.shadowRootOptions={mode:"open"},v[C("elementProperties")]=new Map,v[C("finalized")]=new Map,z==null||z({ReactiveElement:v}),(b.reactiveElementVersions??(b.reactiveElementVersions=[])).push("2.1.1");/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const P=globalThis,T=P.trustedTypes,Q=T?T.createPolicy("lit-html",{createHTML:r=>r}):void 0,lt="$lit$",$=`lit$${Math.random().toFixed(9).slice(2)}$`,ht="?"+$,xt=`<${ht}>`,y=document,N=()=>y.createComment(""),U=r=>r===null||typeof r!="object"&&typeof r!="function",F=Array.isArray,St=r=>F(r)||typeof(r==null?void 0:r[Symbol.iterator])=="function",L=`[ 	
\f\r]`,w=/<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g,X=/-->/g,tt=/>/g,g=RegExp(`>|${L}(?:([^\\s"'>=/]+)(${L}*=${L}*(?:[^ 	
\f\r"'\`<>=]|("|')|))|$)`,"g"),et=/'/g,st=/"/g,ct=/^(?:script|style|textarea|title)$/i,Et=r=>(t,...e)=>({_$litType$:r,strings:t,values:e}),j=Et(1),x=Symbol.for("lit-noChange"),d=Symbol.for("lit-nothing"),it=new WeakMap,m=y.createTreeWalker(y,129);function dt(r,t){if(!F(r)||!r.hasOwnProperty("raw"))throw Error("invalid template strings array");return Q!==void 0?Q.createHTML(t):t}const wt=(r,t)=>{const e=r.length-1,s=[];let i,o=t===2?"<svg>":t===3?"<math>":"",n=w;for(let l=0;l<e;l++){const a=r[l];let c,p,h=-1,f=0;for(;f<a.length&&(n.lastIndex=f,p=n.exec(a),p!==null);)f=n.lastIndex,n===w?p[1]==="!--"?n=X:p[1]!==void 0?n=tt:p[2]!==void 0?(ct.test(p[2])&&(i=RegExp("</"+p[2],"g")),n=g):p[3]!==void 0&&(n=g):n===g?p[0]===">"?(n=i??w,h=-1):p[1]===void 0?h=-2:(h=n.lastIndex-p[2].length,c=p[1],n=p[3]===void 0?g:p[3]==='"'?st:et):n===st||n===et?n=g:n===X||n===tt?n=w:(n=g,i=void 0);const u=n===g&&r[l+1].startsWith("/>")?" ":"";o+=n===w?a+xt:h>=0?(s.push(c),a.slice(0,h)+lt+a.slice(h)+$+u):a+$+(h===-2?l:u)}return[dt(r,o+(r[e]||"<?>")+(t===2?"</svg>":t===3?"</math>":"")),s]};class k{constructor({strings:t,_$litType$:e},s){let i;this.parts=[];let o=0,n=0;const l=t.length-1,a=this.parts,[c,p]=wt(t,e);if(this.el=k.createElement(c,s),m.currentNode=this.el.content,e===2||e===3){const h=this.el.content.firstChild;h.replaceWith(...h.childNodes)}for(;(i=m.nextNode())!==null&&a.length<l;){if(i.nodeType===1){if(i.hasAttributes())for(const h of i.getAttributeNames())if(h.endsWith(lt)){const f=p[n++],u=i.getAttribute(h).split($),H=/([.?@])?(.*)/.exec(f);a.push({type:1,index:o,name:H[2],strings:u,ctor:H[1]==="."?Pt:H[1]==="?"?Nt:H[1]==="@"?Ut:R}),i.removeAttribute(h)}else h.startsWith($)&&(a.push({type:6,index:o}),i.removeAttribute(h));if(ct.test(i.tagName)){const h=i.textContent.split($),f=h.length-1;if(f>0){i.textContent=T?T.emptyScript:"";for(let u=0;u<f;u++)i.append(h[u],N()),m.nextNode(),a.push({type:2,index:++o});i.append(h[f],N())}}}else if(i.nodeType===8)if(i.data===ht)a.push({type:2,index:o});else{let h=-1;for(;(h=i.data.indexOf($,h+1))!==-1;)a.push({type:7,index:o}),h+=$.length-1}o++}}static createElement(t,e){const s=y.createElement("template");return s.innerHTML=t,s}}function S(r,t,e=r,s){var n,l;if(t===x)return t;let i=s!==void 0?(n=e._$Co)==null?void 0:n[s]:e._$Cl;const o=U(t)?void 0:t._$litDirective$;return(i==null?void 0:i.constructor)!==o&&((l=i==null?void 0:i._$AO)==null||l.call(i,!1),o===void 0?i=void 0:(i=new o(r),i._$AT(r,e,s)),s!==void 0?(e._$Co??(e._$Co=[]))[s]=i:e._$Cl=i),i!==void 0&&(t=S(r,i._$AS(r,t.values),i,s)),t}class Ct{constructor(t,e){this._$AV=[],this._$AN=void 0,this._$AD=t,this._$AM=e}get parentNode(){return this._$AM.parentNode}get _$AU(){return this._$AM._$AU}u(t){const{el:{content:e},parts:s}=this._$AD,i=((t==null?void 0:t.creationScope)??y).importNode(e,!0);m.currentNode=i;let o=m.nextNode(),n=0,l=0,a=s[0];for(;a!==void 0;){if(n===a.index){let c;a.type===2?c=new O(o,o.nextSibling,this,t):a.type===1?c=new a.ctor(o,a.name,a.strings,this,t):a.type===6&&(c=new kt(o,this,t)),this._$AV.push(c),a=s[++l]}n!==(a==null?void 0:a.index)&&(o=m.nextNode(),n++)}return m.currentNode=y,i}p(t){let e=0;for(const s of this._$AV)s!==void 0&&(s.strings!==void 0?(s._$AI(t,s,e),e+=s.strings.length-2):s._$AI(t[e])),e++}}class O{get _$AU(){var t;return((t=this._$AM)==null?void 0:t._$AU)??this._$Cv}constructor(t,e,s,i){this.type=2,this._$AH=d,this._$AN=void 0,this._$AA=t,this._$AB=e,this._$AM=s,this.options=i,this._$Cv=(i==null?void 0:i.isConnected)??!0}get parentNode(){let t=this._$AA.parentNode;const e=this._$AM;return e!==void 0&&(t==null?void 0:t.nodeType)===11&&(t=e.parentNode),t}get startNode(){return this._$AA}get endNode(){return this._$AB}_$AI(t,e=this){t=S(this,t,e),U(t)?t===d||t==null||t===""?(this._$AH!==d&&this._$AR(),this._$AH=d):t!==this._$AH&&t!==x&&this._(t):t._$litType$!==void 0?this.$(t):t.nodeType!==void 0?this.T(t):St(t)?this.k(t):this._(t)}O(t){return this._$AA.parentNode.insertBefore(t,this._$AB)}T(t){this._$AH!==t&&(this._$AR(),this._$AH=this.O(t))}_(t){this._$AH!==d&&U(this._$AH)?this._$AA.nextSibling.data=t:this.T(y.createTextNode(t)),this._$AH=t}$(t){var o;const{values:e,_$litType$:s}=t,i=typeof s=="number"?this._$AC(t):(s.el===void 0&&(s.el=k.createElement(dt(s.h,s.h[0]),this.options)),s);if(((o=this._$AH)==null?void 0:o._$AD)===i)this._$AH.p(e);else{const n=new Ct(i,this),l=n.u(this.options);n.p(e),this.T(l),this._$AH=n}}_$AC(t){let e=it.get(t.strings);return e===void 0&&it.set(t.strings,e=new k(t)),e}k(t){F(this._$AH)||(this._$AH=[],this._$AR());const e=this._$AH;let s,i=0;for(const o of t)i===e.length?e.push(s=new O(this.O(N()),this.O(N()),this,this.options)):s=e[i],s._$AI(o),i++;i<e.length&&(this._$AR(s&&s._$AB.nextSibling,i),e.length=i)}_$AR(t=this._$AA.nextSibling,e){var s;for((s=this._$AP)==null?void 0:s.call(this,!1,!0,e);t!==this._$AB;){const i=t.nextSibling;t.remove(),t=i}}setConnected(t){var e;this._$AM===void 0&&(this._$Cv=t,(e=this._$AP)==null||e.call(this,t))}}class R{get tagName(){return this.element.tagName}get _$AU(){return this._$AM._$AU}constructor(t,e,s,i,o){this.type=1,this._$AH=d,this._$AN=void 0,this.element=t,this.name=e,this._$AM=i,this.options=o,s.length>2||s[0]!==""||s[1]!==""?(this._$AH=Array(s.length-1).fill(new String),this.strings=s):this._$AH=d}_$AI(t,e=this,s,i){const o=this.strings;let n=!1;if(o===void 0)t=S(this,t,e,0),n=!U(t)||t!==this._$AH&&t!==x,n&&(this._$AH=t);else{const l=t;let a,c;for(t=o[0],a=0;a<o.length-1;a++)c=S(this,l[s+a],e,a),c===x&&(c=this._$AH[a]),n||(n=!U(c)||c!==this._$AH[a]),c===d?t=d:t!==d&&(t+=(c??"")+o[a+1]),this._$AH[a]=c}n&&!i&&this.j(t)}j(t){t===d?this.element.removeAttribute(this.name):this.element.setAttribute(this.name,t??"")}}class Pt extends R{constructor(){super(...arguments),this.type=3}j(t){this.element[this.name]=t===d?void 0:t}}class Nt extends R{constructor(){super(...arguments),this.type=4}j(t){this.element.toggleAttribute(this.name,!!t&&t!==d)}}class Ut extends R{constructor(t,e,s,i,o){super(t,e,s,i,o),this.type=5}_$AI(t,e=this){if((t=S(this,t,e,0)??d)===x)return;const s=this._$AH,i=t===d&&s!==d||t.capture!==s.capture||t.once!==s.once||t.passive!==s.passive,o=t!==d&&(s===d||i);i&&this.element.removeEventListener(this.name,this,s),o&&this.element.addEventListener(this.name,this,t),this._$AH=t}handleEvent(t){var e;typeof this._$AH=="function"?this._$AH.call(((e=this.options)==null?void 0:e.host)??this.element,t):this._$AH.handleEvent(t)}}class kt{constructor(t,e,s){this.element=t,this.type=6,this._$AN=void 0,this._$AM=e,this.options=s}get _$AU(){return this._$AM._$AU}_$AI(t){S(this,t)}}const D=P.litHtmlPolyfillSupport;D==null||D(k,O),(P.litHtmlVersions??(P.litHtmlVersions=[])).push("3.3.1");const Ot=(r,t,e)=>{const s=(e==null?void 0:e.renderBefore)??t;let i=s._$litPart$;if(i===void 0){const o=(e==null?void 0:e.renderBefore)??null;s._$litPart$=i=new O(t.insertBefore(N(),o),o,void 0,e??{})}return i._$AI(r),i};/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const _=globalThis;class A extends v{constructor(){super(...arguments),this.renderOptions={host:this},this._$Do=void 0}createRenderRoot(){var e;const t=super.createRenderRoot();return(e=this.renderOptions).renderBefore??(e.renderBefore=t.firstChild),t}update(t){const e=this.render();this.hasUpdated||(this.renderOptions.isConnected=this.isConnected),super.update(t),this._$Do=Ot(e,this.renderRoot,this.renderOptions)}connectedCallback(){var t;super.connectedCallback(),(t=this._$Do)==null||t.setConnected(!0)}disconnectedCallback(){var t;super.disconnectedCallback(),(t=this._$Do)==null||t.setConnected(!1)}render(){return x}}var rt;A._$litElement$=!0,A.finalized=!0,(rt=_.litElementHydrateSupport)==null||rt.call(_,{LitElement:A});const B=_.litElementPolyfillSupport;B==null||B({LitElement:A});(_.litElementVersions??(_.litElementVersions=[])).push("4.2.1");class W extends A{get beltStyles(){const t={White:{bg:"#ffffff",text:"#333333",border:"#cccccc"},Grey:{bg:"#9e9e9e",text:"#ffffff",border:"#757575"},Yellow:{bg:"#ffeb3b",text:"#333333",border:"#fbc02d"},Orange:{bg:"#ff9800",text:"#ffffff",border:"#e65100"},Green:{bg:"#4caf50",text:"#ffffff",border:"#2e7d32"},Blue:{bg:"#2196f3",text:"#ffffff",border:"#1565c0"},Purple:{bg:"#9c27b0",text:"#ffffff",border:"#6a1b9a"},Brown:{bg:"#795548",text:"#ffffff",border:"#4e342e"},Black:{bg:"#212121",text:"#ffffff",border:"#000000"}};return t[this.beltRank]||t.White}render(){const{bg:t,text:e,border:s}=this.beltStyles;return j`
          <div class="user-badge" style="--belt-bg: ${t}; --belt-text: ${e}; --belt-border: ${s}">
            <div class="user-icon">${this.userInitial}</div>
            <span class="user-name">${this.userName}</span>
            <span class="belt-tag">${this.beltRank} belt</span>
          </div>
        `}}E(W,"properties",{userName:{type:String,attribute:"user-name"},userInitial:{type:String,attribute:"user-initial"},beltRank:{type:String,attribute:"belt-rank"}}),E(W,"styles",ot`
        .user-badge {
            display: inline-flex;
            align-items: center;
            gap: 10px;
            padding: 6px 14px;
            border-radius: 25px;
            font-family: system-ui, -apple-system, sans-serif;
            
            /* Use the CSS variables set in render() */
            background-color: var(--belt-bg);
            color: var(--belt-text);
            border: 2px solid var(--belt-border);
            
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .user-icon {
            width: 28px;
            height: 28px;
            background: white;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: bold;
            font-size: 14px;
            /* Icon text matches the belt color for a nice look */
            color: var(--belt-bg);
            border: 1px solid rgba(0,0,0,0.1);
        }

        .user-name {
            font-weight: 600;
            font-size: 15px;
        }

        .belt-tag {
            font-size: 11px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            padding-left: 8px;
            border-left: 1px solid rgba(0,0,0,0.2);
        }
    `);customElements.define("user-badge",W);class V extends A{constructor(){super(),this.firstName="",this.lastName="",this.email="",this.phoneNumber="",this.belt="White",this.memberSince="",this.lastLogin=""}get beltConfig(){const t=this.belt?this.belt.charAt(0).toUpperCase()+this.belt.slice(1).toLowerCase():"White",e={White:{main:"#ffffff",text:"#333333",accent:"#667eea"},Grey:{main:"#9e9e9e",text:"#ffffff",accent:"#757575"},Yellow:{main:"#ffeb3b",text:"#333333",accent:"#fbc02d"},Orange:{main:"#ff9800",text:"#ffffff",accent:"#ff9800"},Green:{main:"#4caf50",text:"#ffffff",accent:"#4caf50"},Blue:{main:"#2196f3",text:"#ffffff",accent:"#2196f3"},Purple:{main:"#9c27b0",text:"#ffffff",accent:"#9c27b0"},Brown:{main:"#795548",text:"#ffffff",accent:"#795548"},Black:{main:"#212121",text:"#ffffff",accent:"#212121"}};return e[t]||e.White}getInitial(){return this.firstName?this.firstName[0].toUpperCase():"?"}getFullName(){return`${this.firstName} ${this.lastName}`}formatDate(t){if(!t||t==="Never")return"Never";const e=new Date(t);return isNaN(e)?t:e.toLocaleDateString("en-US",{year:"numeric",month:"long",day:"numeric"})}render(){const{main:t,text:e,accent:s}=this.beltConfig;return j`
      <div class="profile-card" style="--belt-color: ${t}; --belt-text: ${e}; --avatar-accent: ${s}">
        <div class="profile-header">
          <div class="profile-avatar">${this.getInitial()}</div>
          <h2 class="profile-name">${this.getFullName()}</h2>
          ${this.belt?j`<div class="profile-belt">${this.belt} Belt</div>`:""}
        </div>
        
        <div class="profile-body">
          <div class="info-grid">
            <div class="info-item">
              <span class="info-label">Email</span>
              <span class="info-value">${this.email}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Phone</span>
              <span class="info-value">${this.phoneNumber||"Not provided"}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Member Since</span>
              <span class="info-value">${this.formatDate(this.memberSince)}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Last Login</span>
              <span class="info-value">${this.formatDate(this.lastLogin)}</span>
            </div>
          </div>
          
          <div class="action-buttons">
            <button class="btn btn-primary" @click=${()=>this.dispatchEvent(new CustomEvent("edit-profile"))}>
              Edit Profile
            </button>
            <button class="btn btn-outline" @click=${()=>this.dispatchEvent(new CustomEvent("change-password"))}>
              Change Password
            </button>
          </div>
        </div>
      </div>
    `}}E(V,"properties",{firstName:{type:String,attribute:"first-name"},lastName:{type:String,attribute:"last-name"},email:{type:String},phoneNumber:{type:String,attribute:"phone-number"},belt:{type:String},memberSince:{type:String,attribute:"member-since"},lastLogin:{type:String,attribute:"last-login"}}),E(V,"styles",ot`
    :host { display: block; margin: 20px 0; }
    .profile-card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.08);
      overflow: hidden;
      border: 1px solid #e9ecef;
    }
    .profile-header {
      /* Dynamic Gradient based on belt color */
      background: linear-gradient(135deg, var(--belt-color) 0%, #444 150%);
      color: var(--belt-text);
      padding: 32px 24px;
      text-align: center;
      transition: all 0.5s ease;
    }
    /* Special case for white belt header visibility */
    .profile-card[style*="--belt-color: #ffffff"] .profile-header {
        background: #f8f9fa;
        border-bottom: 1px solid #dee2e6;
    }
    .profile-avatar {
      width: 80px; height: 80px;
      border-radius: 50%;
      background: white;
      margin: 0 auto 16px;
      display: flex; align-items: center; justify-content: center;
      color: var(--avatar-accent);
      font-size: 32px; font-weight: bold;
      border: 4px solid rgba(0,0,0,0.1);
    }
    .profile-name { font-size: 24px; margin: 0 0 8px 0; }
    .profile-belt {
      display: inline-block;
      background: rgba(0,0,0,0.1);
      padding: 4px 16px;
      border-radius: 20px;
      font-size: 13px; font-weight: bold;
      text-transform: uppercase;
    }
    .profile-body { padding: 24px; }
    .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
    .info-label { font-size: 11px; color: #6c757d; font-weight: bold; text-transform: uppercase; }
    .info-value { display: block; font-size: 15px; color: #212529; }
    .action-buttons { display: flex; gap: 12px; margin-top: 24px; }
    .btn { flex: 1; padding: 12px; border-radius: 8px; border: none; font-weight: 600; cursor: pointer; }
    .btn-primary { background: #667eea; color: white; }
    .btn-outline { background: transparent; color: #667eea; border: 2px solid #667eea; }
  `);customElements.define("profile-card",V);
