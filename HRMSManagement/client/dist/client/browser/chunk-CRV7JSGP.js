import{d as ft,e as mt,q as Tt,r as Ct}from"./chunk-PF5NYNN6.js";import{h as vt}from"./chunk-AQ6NZXXU.js";import{b as gt}from"./chunk-NSPFTXK5.js";import{a as N}from"./chunk-C2LGCBEQ.js";import{C as V,V as ht,da as W,ea as S,ja as _t,k as bt,p as pt,q as k,r as ut,s as f,y as K}from"./chunk-YVXIKYMN.js";import{g as ct,i as M,m as dt,n as z,r as q,u as E}from"./chunk-YIFEAXLI.js";import{$,$b as ot,Ab as tt,Bb as et,Bc as y,Cb as _,Cc as rt,Db as m,Eb as x,Fb as it,Gb as nt,Hb as H,Ib as w,Mb as I,Nb as s,Ob as R,Pb as D,Sb as h,Tb as C,U as j,Ub as d,V as U,Vb as b,W as G,Wa as p,Zb as at,ec as L,fa as v,ga as T,gb as P,gc as lt,hb as J,hc as st,ia as F,kb as O,mb as r,pa as B,sb as u,tb as c,vb as X,wb as Y,xb as Q,yb as g,zb as Z}from"./chunk-MNLHA4RB.js";var yt=["content"],Vt=["header"],St=["lefticon"],$t=["righticon"],Ft=["closeicon"],xt=["*"];function Bt(e,o){e&1&&H(0)}function Et(e,o){if(e&1&&(it(0),r(1,Bt,1,0,"ng-container",3),nt()),e&2){let t=s(2);p(),c("ngTemplateOutlet",t.contentTemplate||t._contentTemplate)}}function At(e,o){if(e&1&&(_(0,"div",1),D(1),r(2,Et,2,1,"ng-container",2),m()),e&2){let t=s();c("hidden",!t.selected),u("id",t.tabView.getTabContentId(t.id))("aria-hidden",!t.selected)("aria-labelledby",t.tabView.getTabHeaderActionId(t.id))("data-pc-name","tabpanel"),p(2),c("ngIf",(t.contentTemplate||t._contentTemplate)&&(t.cache?t.loaded:t.selected))}}var Pt=["previousicon"],Ot=["nexticon"],Qt=["navbar"],Ht=["prevBtn"],Rt=["nextBtn"],Dt=["inkbar"],Lt=["elementToObserve"],Mt=e=>({"p-tablist-viewport":e}),zt=(e,o)=>({"p-tab":!0,"p-tab-active":e,"p-disabled":o});function qt(e,o){e&1&&x(0,"ChevronLeftIcon"),e&2&&u("aria-hidden",!0)}function Kt(e,o){}function Wt(e,o){e&1&&r(0,Kt,0,0,"ng-template")}function Nt(e,o){if(e&1){let t=w();_(0,"button",12,3),I("click",function(){v(t);let n=s();return T(n.navBackward())}),r(2,qt,1,1,"ChevronLeftIcon",13)(3,Wt,1,0,null,14),m()}if(e&2){let t=s();u("tabindex",t.tabindex)("aria-label",t.prevButtonAriaLabel),p(2),c("ngIf",!t.previousIconTemplate&&!t._previousIconTemplate),p(),c("ngTemplateOutlet",t.previousIconTemplate&&t._previousIconTemplate)}}function jt(e,o){e&1&&H(0)}function Ut(e,o){if(e&1&&r(0,jt,1,0,"ng-container",14),e&2){let t=s(2).$implicit;c("ngTemplateOutlet",t.headerTemplate||t._headerTemplate)}}function Gt(e,o){}function Jt(e,o){e&1&&r(0,Gt,0,0,"ng-template")}function Xt(e,o){if(e&1&&r(0,Jt,1,0,null,14),e&2){let t=s(3).$implicit;c("ngTemplateOutlet",t.leftIconTemplate||t._leftIconTemplate)}}function Yt(e,o){if(e&1&&x(0,"span",17),e&2){let t=s(3).$implicit;c("ngClass",t.leftIcon)}}function Zt(e,o){}function te(e,o){e&1&&r(0,Zt,0,0,"ng-template")}function ee(e,o){if(e&1&&r(0,te,1,0,null,14),e&2){let t=s(3).$implicit;c("ngTemplateOutlet",t.rightIconTemplate||t._rightIconTemplate)}}function ie(e,o){if(e&1&&x(0,"span",18),e&2){let t=s(3).$implicit;c("ngClass",t.rightIcon)}}function ne(e,o){}function ae(e,o){e&1&&r(0,ne,0,0,"ng-template")}function oe(e,o){if(e&1&&r(0,ae,1,0,null,14),e&2){let t=s(4).$implicit;c("ngTemplateOutlet",t.closeIconTemplate||t._closeIconTemplate)}}function le(e,o){if(e&1){let t=w();_(0,"TimesIcon",19),I("click",function(n){v(t);let a=s(4).$implicit,l=s();return T(l.close(n,a))}),m()}}function se(e,o){if(e&1&&r(0,oe,1,1)(1,le,1,0,"TimesIcon"),e&2){let t=s(3).$implicit;g(t.closeIconTemplate||t._closeIconTemplate?0:1)}}function re(e,o){if(e&1&&(r(0,Xt,1,1)(1,Yt,1,1,"span",17),at(2),r(3,ee,1,1)(4,ie,1,1,"span",18)(5,se,2,1)),e&2){let t=s(2).$implicit;g(t.leftIconTemplate||t._leftIconTemplate?0:t.leftIcon&&!t.leftIconTemplate&&!t._leftIconTemplate?1:-1),p(2),ot(" ",t.header," "),p(),g(t.rightIconTemplate||t._rightIconTemplate?3:t.rightIcon&&!t.rightIconTemplate&&!t._rightIconTemplate?4:-1),p(2),g(t.closable?5:-1)}}function ce(e,o){if(e&1){let t=w();_(0,"button",15),I("click",function(n){v(t);let a=s().$implicit,l=s();return T(l.open(n,a))})("keydown",function(n){v(t);let a=s().$implicit,l=s();return T(l.onTabKeyDown(n,a))}),r(1,Ut,1,1,"ng-container")(2,re,6,4),m(),x(3,"span",16,4)}if(e&2){let t=s(),i=t.$implicit,n=t.$index,a=s();Q(i.headerStyleClass),c("ngClass",st(22,zt,i.selected,i.disabled))("ngStyle",i.headerStyle)("pTooltip",i.tooltip)("tooltipPosition",i.tooltipPosition)("positionStyle",i.tooltipPositionStyle)("tooltipStyleClass",i.tooltipStyleClass)("disabled",i.disabled),u("role","tab")("id",a.getTabHeaderActionId(i.id))("aria-controls",a.getTabContentId(i.id))("aria-selected",i.selected)("tabindex",i.disabled||!i.selected?"-1":a.tabindex)("aria-disabled",i.disabled)("data-pc-index",n)("data-p-disabled",i.disabled)("data-pc-section","headeraction")("data-p-active",i.selected),p(),g(i.headerTemplate||i._headerTemplate?1:2),p(2),u("aria-hidden",!0)("data-pc-section","inkbar")}}function de(e,o){if(e&1&&r(0,ce,5,25),e&2){let t=o.$implicit;g(t.closed?-1:0)}}function be(e,o){}function pe(e,o){e&1&&r(0,be,0,0,"ng-template")}function ue(e,o){if(e&1&&r(0,pe,1,0,null,14),e&2){let t=s(2);c("ngTemplateOutlet",t.nextIconTemplate||t._nextIconTemplate)}}function he(e,o){e&1&&x(0,"ChevronRightIcon"),e&2&&u("aria-hidden",!0)}function _e(e,o){if(e&1){let t=w();_(0,"button",20,5),I("click",function(){v(t);let n=s();return T(n.navForward())}),r(2,ue,1,1)(3,he,1,1,"ChevronRightIcon"),m()}if(e&2){let t=s();u("tabindex",t.tabindex)("aria-label",t.nextButtonAriaLabel),p(2),g(t.nextIconTemplate||t._nextIconTemplate?2:3)}}var fe=({dt:e})=>`
.p-tabs {
    display: flex;
    flex-direction: column;
}

.p-tablist {
    display: flex;
    position: relative;
}

.p-tabs-scrollable > .p-tablist {
    overflow: hidden;
}

.p-tablist-viewport {
    overflow-x: auto;
    overflow-y: hidden;
    scroll-behavior: smooth;
    scrollbar-width: none;
    overscroll-behavior: contain auto;
}

.p-tablist-viewport::-webkit-scrollbar {
    display: none;
}

.p-tablist-tab-list {
    position: relative;
    display: flex;
    background: ${e("tabs.tablist.background")};
    border-style: solid;
    border-color: ${e("tabs.tablist.border.color")};
    border-width: ${e("tabs.tablist.border.width")};
}

.p-tablist-content {
    flex-grow: 1;
}

.p-tablist-nav-button {
    all: unset;
    position: absolute !important;
    flex-shrink: 0;
    top: 0;
    z-index: 2;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: ${e("tabs.nav.button.background")};
    color: ${e("tabs.nav.button.color")};
    width: ${e("tabs.nav.button.width")};
    transition: color ${e("tabs.transition.duration")}, outline-color ${e("tabs.transition.duration")}, box-shadow ${e("tabs.transition.duration")};
    box-shadow: ${e("tabs.nav.button.shadow")};
    outline-color: transparent;
    cursor: pointer;
}

.p-tablist-nav-button:focus-visible {
    z-index: 1;
    box-shadow: ${e("tabs.nav.button.focus.ring.shadow")};
    outline: ${e("tabs.nav.button.focus.ring.width")} ${e("tabs.nav.button.focus.ring.style")} ${e("tabs.nav.button.focus.ring.color")};
    outline-offset: ${e("tabs.nav.button.focus.ring.offset")};
}

.p-tablist-nav-button:hover {
    color: ${e("tabs.nav.button.hover.color")};
}

.p-tablist-prev-button {
    left: 0;
}

.p-tablist-next-button {
    right: 0;
}

.p-tab {
    display: flex;
    align-items: center;
    flex-shrink: 0;
    cursor: pointer;
    user-select: none;
    position: relative;
    border-style: solid;
    white-space: nowrap;
    gap: ${e("tabs.tab.gap")};
    background: ${e("tabs.tab.background")};
    border-width: ${e("tabs.tab.border.width")};
    border-color: ${e("tabs.tab.border.color")};
    color: ${e("tabs.tab.color")};
    padding: ${e("tabs.tab.padding")};
    font-weight: ${e("tabs.tab.font.weight")};
    transition: background ${e("tabs.transition.duration")}, border-color ${e("tabs.transition.duration")}, color ${e("tabs.transition.duration")}, outline-color ${e("tabs.transition.duration")}, box-shadow ${e("tabs.transition.duration")};
    margin: ${e("tabs.tab.margin")};
    outline-color: transparent;
}

.p-tab:not(.p-disabled):focus-visible {
    z-index: 1;
    box-shadow: ${e("tabs.tab.focus.ring.shadow")};
    outline: ${e("tabs.tab.focus.ring.width")} ${e("tabs.tab.focus.ring.style")} ${e("tabs.tab.focus.ring.color")};
    outline-offset: ${e("tabs.tab.focus.ring.offset")};
}

.p-tab:not(.p-tab-active):not(.p-disabled):hover {
    background: ${e("tabs.tab.hover.background")};
    border-color: ${e("tabs.tab.hover.border.color")};
    color: ${e("tabs.tab.hover.color")};
}

.p-tab-active {
    background: ${e("tabs.tab.active.background")};
    border-color: ${e("tabs.tab.active.border.color")};
    color: ${e("tabs.tab.active.color")};
}

.p-tabpanels {
    background: ${e("tabs.tabpanel.background")};
    color: ${e("tabs.tabpanel.color")};
    padding: ${e("tabs.tabpanel.padding")};
    outline: 0 none;
}

.p-tabpanel:focus-visible {
    box-shadow: ${e("tabs.tabpanel.focus.ring.shadow")};
    outline: ${e("tabs.tabpanel.focus.ring.width")} ${e("tabs.tabpanel.focus.ring.style")} ${e("tabs.tabpanel.focus.ring.color")};
    outline-offset: ${e("tabs.tabpanel.focus.ring.offset")};
}

.p-tablist-active-bar {
    z-index: 1;
    display: block;
    position: absolute;
    bottom: ${e("tabs.active.bar.bottom")};
    height: ${e("tabs.active.bar.height")};
    background: ${e("tabs.active.bar.background")};
    transition: 250ms cubic-bezier(0.35, 0, 0.25, 1);
}
`,me={root:({props:e})=>["p-tabs p-component",{"p-tabs-scrollable":e.scrollable}]},A=(()=>{class e extends _t{name="tabs";theme=fe;classes=me;static \u0275fac=(()=>{let t;return function(n){return(t||(t=F(e)))(n||e)}})();static \u0275prov=U({token:e,factory:e.\u0275fac})}return e})();var wt=(()=>{class e extends N{closable=!1;get headerStyle(){return this._headerStyle}set headerStyle(t){this._headerStyle=t,this.tabView.cd.markForCheck()}get headerStyleClass(){return this._headerStyleClass}set headerStyleClass(t){this._headerStyleClass=t,this.tabView.cd.markForCheck()}cache=!0;tooltip;tooltipPosition="top";tooltipPositionStyle="absolute";tooltipStyleClass;get selected(){return!!this._selected}set selected(t){this._selected=t,this.loaded||this.cd.detectChanges(),t&&(this.loaded=!0)}get disabled(){return!!this._disabled}set disabled(t){this._disabled=t,this.tabView.cd.markForCheck()}get header(){return this._header}set header(t){this._header=t,Promise.resolve().then(()=>{this.tabView.updateInkBar(),this.tabView.cd.markForCheck()})}get leftIcon(){return this._leftIcon}set leftIcon(t){this._leftIcon=t,this.tabView.cd.markForCheck()}get rightIcon(){return this._rightIcon}set rightIcon(t){this._rightIcon=t,this.tabView.cd.markForCheck()}closed=!1;_headerStyle;_headerStyleClass;_selected;_disabled;_header;_leftIcon;_rightIcon=void 0;loaded=!1;id=ht("pn_id_");contentTemplate;headerTemplate;leftIconTemplate;rightIconTemplate;closeIconTemplate;templates;tabView=$(j(()=>It));_componentStyle=$(A);_headerTemplate;_contentTemplate;_rightIconTemplate;_leftIconTemplate;_closeIconTemplate;ngAfterContentInit(){this.templates.forEach(t=>{switch(t.getType()){case"header":this._headerTemplate=t.template;break;case"content":this._contentTemplate=t.template;break;case"righticon":this._rightIconTemplate=t.template;break;case"lefticon":this._leftIconTemplate=t.template;break;case"closeicon":this._closeIconTemplate=t.template;break;default:this._contentTemplate=t.template;break}})}static \u0275fac=(()=>{let t;return function(n){return(t||(t=F(e)))(n||e)}})();static \u0275cmp=P({type:e,selectors:[["p-tabPanel"],["p-tabpanel"]],contentQueries:function(i,n,a){if(i&1&&(h(a,yt,5),h(a,Vt,5),h(a,St,5),h(a,$t,5),h(a,Ft,5),h(a,W,4)),i&2){let l;d(l=b())&&(n.contentTemplate=l.first),d(l=b())&&(n.headerTemplate=l.first),d(l=b())&&(n.leftIconTemplate=l.first),d(l=b())&&(n.rightIconTemplate=l.first),d(l=b())&&(n.closeIconTemplate=l.first),d(l=b())&&(n.templates=l)}},inputs:{closable:[2,"closable","closable",y],headerStyle:"headerStyle",headerStyleClass:"headerStyleClass",cache:[2,"cache","cache",y],tooltip:"tooltip",tooltipPosition:"tooltipPosition",tooltipPositionStyle:"tooltipPositionStyle",tooltipStyleClass:"tooltipStyleClass",selected:"selected",disabled:"disabled",header:"header",leftIcon:"leftIcon",rightIcon:"rightIcon"},features:[L([A]),O],ngContentSelectors:xt,decls:1,vars:1,consts:[["class","p-tabview-panel","role","tabpanel",3,"hidden",4,"ngIf"],["role","tabpanel",1,"p-tabview-panel",3,"hidden"],[4,"ngIf"],[4,"ngTemplateOutlet"]],template:function(i,n){i&1&&(R(),r(0,At,3,6,"div",0)),i&2&&c("ngIf",!n.closed)},dependencies:[q,M,z,S],encapsulation:2})}return e})(),It=(()=>{class e extends N{get hostClass(){return this.styleClass}get hostStyle(){return this.style}style;styleClass;controlClose;scrollable;get activeIndex(){return this._activeIndex}set activeIndex(t){if(this._activeIndex=t,this.preventActiveIndexPropagation){this.preventActiveIndexPropagation=!1;return}this.tabs&&this.tabs.length&&this._activeIndex!=null&&this.tabs.length>this._activeIndex&&(this.findSelectedTab().selected=!1,this.tabs[this._activeIndex].selected=!0,this.tabChanged=!0,this.updateScrollBar(t))}selectOnFocus=!1;nextButtonAriaLabel;prevButtonAriaLabel;autoHideButtons=!0;tabindex=0;onChange=new B;onClose=new B;activeIndexChange=new B;content;navbar;prevBtn;nextBtn;inkbar;tabPanels;initialized;tabs;_activeIndex;preventActiveIndexPropagation;tabChanged;backwardIsDisabled=!0;forwardIsDisabled=!1;tabChangesSubscription;resizeObserver;container;list;buttonVisible;elementToObserve;previousIconTemplate;nextIconTemplate;_previousIconTemplate;_nextIconTemplate;_componentStyle=$(A);templates;ngOnInit(){super.ngOnInit(),console.log("TabView component is deprecated as of v18. Use Tabs component instead.")}ngAfterContentInit(){this.initTabs(),this.tabChangesSubscription=this.tabPanels.changes.subscribe(t=>{this.initTabs(),this.refreshButtonState()}),this.templates.forEach(t=>{switch(t.getType()){case"previousicon":this._previousIconTemplate=t.template;break;case"nexticon":this._nextIconTemplate=t.template;break}})}ngAfterViewInit(){super.ngAfterViewInit(),E(this.platformId)&&this.autoHideButtons&&this.bindResizeObserver()}bindResizeObserver(){this.container=k(this.el.nativeElement,'[data-pc-section="navcontent"]'),this.list=k(this.el.nativeElement,'[data-pc-section="nav"]'),this.resizeObserver=new ResizeObserver(()=>{this.list.offsetWidth>=this.container.offsetWidth?this.buttonVisible=!0:this.buttonVisible=!1,this.updateButtonState(),this.cd.detectChanges()}),this.resizeObserver.observe(this.container)}unbindResizeObserver(){this.resizeObserver.unobserve(this.elementToObserve.nativeElement),this.resizeObserver=null}ngAfterViewChecked(){E(this.platformId)&&this.tabChanged&&(this.updateInkBar(),this.tabChanged=!1)}ngOnDestroy(){this.tabChangesSubscription&&this.tabChangesSubscription.unsubscribe(),this.resizeObserver&&this.unbindResizeObserver(),super.ngOnDestroy()}getTabHeaderActionId(t){return`${t}_header_action`}getTabContentId(t){return`${t}_content`}initTabs(){this.tabs=this.tabPanels.toArray(),!this.findSelectedTab()&&this.tabs.length&&(this.activeIndex!=null&&this.tabs.length>this.activeIndex?this.tabs[this.activeIndex].selected=!0:this.tabs[0].selected=!0,this.tabChanged=!0),this.cd.markForCheck()}onTabKeyDown(t,i){switch(t.code){case"ArrowLeft":this.onTabArrowLeftKey(t);break;case"ArrowRight":this.onTabArrowRightKey(t);break;case"Home":this.onTabHomeKey(t);break;case"End":this.onTabEndKey(t);break;case"PageDown":this.onTabEndKey(t);break;case"PageUp":this.onTabHomeKey(t);break;case"Enter":case"Space":this.open(t,i);break;default:break}}onTabArrowLeftKey(t){let i=this.findPrevHeaderAction(t.currentTarget),n=f(i,"data-pc-index");i?this.changeFocusedTab(t,i,n):this.onTabEndKey(t),t.preventDefault()}onTabArrowRightKey(t){let i=this.findNextHeaderAction(t.currentTarget),n=f(i,"data-pc-index");i?this.changeFocusedTab(t,i,n):this.onTabHomeKey(t),t.preventDefault()}onTabHomeKey(t){let i=this.findFirstHeaderAction(),n=f(i,"data-pc-index");this.changeFocusedTab(t,i,n),t.preventDefault()}onTabEndKey(t){let i=this.findLastHeaderAction(),n=f(i,"data-pc-index");this.changeFocusedTab(t,i,n),t.preventDefault()}changeFocusedTab(t,i,n){if(i&&(ut(i),i.scrollIntoView({block:"nearest"}),this.selectOnFocus)){let a=this.tabs[n];this.open(t,a)}}findNextHeaderAction(t,i=!1){let n=i?t:t.nextElementSibling;return n?f(n,"data-p-disabled")||f(n,"data-pc-section")==="inkbar"?this.findNextHeaderAction(n):n:null}findPrevHeaderAction(t,i=!1){let n=i?t:t.previousElementSibling;return n?f(n,"data-p-disabled")||f(n,"data-pc-section")==="inkbar"?this.findPrevHeaderAction(n):n:null}findFirstHeaderAction(){let t=this.navbar.nativeElement.firstElementChild;return this.findNextHeaderAction(t,!0)}findLastHeaderAction(){let t=this.navbar.nativeElement.lastElementChild,i=f(t,"data-pc-section")==="inkbar"?t.previousElementSibling:t;return this.findPrevHeaderAction(i,!0)}open(t,i){if(i.disabled){t&&t.preventDefault();return}if(!i.selected){let n=this.findSelectedTab();n&&(n.selected=!1),this.tabChanged=!0,i.selected=!0;let a=this.findTabIndex(i);this.preventActiveIndexPropagation=!0,this.activeIndexChange.emit(a),this.onChange.emit({originalEvent:t,index:a}),this.updateScrollBar(a)}t&&t.preventDefault()}close(t,i){this.controlClose?this.onClose.emit({originalEvent:t,index:this.findTabIndex(i),close:()=>{this.closeTab(i)}}):(this.closeTab(i),this.onClose.emit({originalEvent:t,index:this.findTabIndex(i)})),t.stopPropagation()}closeTab(t){if(!t.disabled){if(t.selected){this.tabChanged=!0,t.selected=!1;for(let i=0;i<this.tabs.length;i++){let n=this.tabs[i];if(!n.closed&&!t.disabled){n.selected=!0;break}}}t.closed=!0}}findSelectedTab(){for(let t=0;t<this.tabs.length;t++)if(this.tabs[t].selected)return this.tabs[t];return null}findTabIndex(t){let i=-1;for(let n=0;n<this.tabs.length;n++)if(this.tabs[n]==t){i=n;break}return i}getBlockableElement(){return this.el.nativeElement.children[0]}updateInkBar(){if(E(this.platformId)&&this.navbar){let t=k(this.navbar.nativeElement,'[data-pc-section="headeraction"][data-p-active="true"]');if(!t)return;this.inkbar.nativeElement.style.width=bt(t)+"px",this.inkbar.nativeElement.style.left=K(t).left-K(this.navbar.nativeElement).left+"px"}}updateScrollBar(t){let i=pt(this.navbar.nativeElement,'[data-pc-section="headeraction"]')[t];i&&i.scrollIntoView({block:"nearest"})}updateButtonState(){let t=this.content.nativeElement,{scrollLeft:i,scrollWidth:n}=t,a=V(t);this.backwardIsDisabled=i===0,this.forwardIsDisabled=Math.round(i)===n-a}refreshButtonState(){this.container=k(this.el.nativeElement,'[data-pc-section="navcontent"]'),this.list=k(this.el.nativeElement,'[data-pc-section="nav"]'),this.list.offsetWidth>=this.container.offsetWidth&&(this.list.offsetWidth>=this.container.offsetWidth?this.buttonVisible=!0:this.buttonVisible=!1,this.updateButtonState(),this.cd.markForCheck())}onScroll(t){this.scrollable&&this.updateButtonState(),t.preventDefault()}getVisibleButtonWidths(){return[this.prevBtn?.nativeElement,this.nextBtn?.nativeElement].reduce((t,i)=>i?t+V(i):t,0)}navBackward(){let t=this.content.nativeElement,i=V(t)-this.getVisibleButtonWidths(),n=t.scrollLeft-i;t.scrollLeft=n<=0?0:n}navForward(){let t=this.content.nativeElement,i=V(t)-this.getVisibleButtonWidths(),n=t.scrollLeft+i,a=t.scrollWidth-i;t.scrollLeft=n>=a?a:n}static \u0275fac=(()=>{let t;return function(n){return(t||(t=F(e)))(n||e)}})();static \u0275cmp=P({type:e,selectors:[["p-tabView"],["p-tabview"]],contentQueries:function(i,n,a){if(i&1&&(h(a,Pt,5),h(a,Ot,5),h(a,wt,4),h(a,W,4)),i&2){let l;d(l=b())&&(n.previousIconTemplate=l.first),d(l=b())&&(n.nextIconTemplate=l.first),d(l=b())&&(n.tabPanels=l),d(l=b())&&(n.templates=l)}},viewQuery:function(i,n){if(i&1&&(C(yt,5),C(Qt,5),C(Ht,5),C(Rt,5),C(Dt,5),C(Lt,5)),i&2){let a;d(a=b())&&(n.content=a.first),d(a=b())&&(n.navbar=a.first),d(a=b())&&(n.prevBtn=a.first),d(a=b())&&(n.nextBtn=a.first),d(a=b())&&(n.inkbar=a.first),d(a=b())&&(n.elementToObserve=a.first)}},hostVars:11,hostBindings:function(i,n){i&2&&(u("data-pc-name","tabview"),Y(n.hostStyle),Q(n.hostClass),X("p-tabs",!0)("p-tabs-scrollable",n.scrollable)("p-component",!0))},inputs:{style:"style",styleClass:"styleClass",controlClose:[2,"controlClose","controlClose",y],scrollable:[2,"scrollable","scrollable",y],activeIndex:"activeIndex",selectOnFocus:[2,"selectOnFocus","selectOnFocus",y],nextButtonAriaLabel:"nextButtonAriaLabel",prevButtonAriaLabel:"prevButtonAriaLabel",autoHideButtons:[2,"autoHideButtons","autoHideButtons",y],tabindex:[2,"tabindex","tabindex",rt]},outputs:{onChange:"onChange",onClose:"onClose",activeIndexChange:"activeIndexChange"},features:[L([A]),O],ngContentSelectors:xt,decls:12,vars:7,consts:[["elementToObserve",""],["content",""],["navbar",""],["prevBtn",""],["inkbar",""],["nextBtn",""],[1,"p-tablist"],["class","p-tablist-prev-button p-tablist-nav-button","type","button","pRipple","",3,"click",4,"ngIf"],[1,"p-tablist-content",3,"scroll","ngClass"],["role","tablist",1,"p-tablist-tab-list"],["class","p-tablist-next-button p-tablist-nav-button","type","button","pRipple","",3,"click",4,"ngIf"],[1,"p-tabpanels"],["type","button","pRipple","",1,"p-tablist-prev-button","p-tablist-nav-button",3,"click"],[4,"ngIf"],[4,"ngTemplateOutlet"],["pRipple","",3,"click","keydown","ngClass","ngStyle","pTooltip","tooltipPosition","positionStyle","tooltipStyleClass","disabled"],["role","presentation",1,"p-tablist-active-bar"],[1,"p-tabview-left-icon",3,"ngClass"],[1,"p-tabview-right-icon",3,"ngClass"],[3,"click"],["type","button","pRipple","",1,"p-tablist-next-button","p-tablist-nav-button",3,"click"]],template:function(i,n){if(i&1){let a=w();R(),_(0,"div",6,0),r(2,Nt,4,4,"button",7),_(3,"div",8,1),I("scroll",function(kt){return v(a),T(n.onScroll(kt))}),_(5,"div",9,2),tt(7,de,1,1,null,null,Z),m()(),r(9,_e,4,3,"button",10),m(),_(10,"div",11),D(11),m()}i&2&&(p(2),c("ngIf",n.scrollable&&!n.backwardIsDisabled&&n.autoHideButtons),p(),c("ngClass",lt(5,Mt,n.scrollable)),u("data-pc-section","navcontent"),p(2),u("data-pc-section","nav"),p(2),et(n.tabs),p(2),c("ngIf",n.scrollable&&!n.forwardIsDisabled&&n.buttonVisible))},dependencies:[q,ct,M,z,dt,S,Ct,Tt,vt,gt,ft,mt],encapsulation:2,changeDetection:0})}return e})(),Me=(()=>{class e{static \u0275fac=function(i){return new(i||e)};static \u0275mod=J({type:e});static \u0275inj=G({imports:[It,wt,S,S]})}return e})();export{wt as a,It as b,Me as c};
