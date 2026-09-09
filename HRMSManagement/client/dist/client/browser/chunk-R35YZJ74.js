import{a as ue}from"./chunk-46R4OZ7U.js";import{q as pe,r as le,s as me,t as de,u as ce,v as ge}from"./chunk-PF5NYNN6.js";import{a as te,b as ne,i as ie,k as oe}from"./chunk-AQ6NZXXU.js";import"./chunk-3LTIOXFH.js";import"./chunk-NSPFTXK5.js";import"./chunk-OFLOCURC.js";import{c as J,e as Q,h as X,o as Z}from"./chunk-U5BEXTDZ.js";import{a as ae,b as se}from"./chunk-BEIRUKNL.js";import{a as re}from"./chunk-QOD7SMYX.js";import{a as ee}from"./chunk-C2LGCBEQ.js";import{da as Y,ea as _,ja as q}from"./chunk-YVXIKYMN.js";import{g as H,m as U,o as G,p as K,r as C}from"./chunk-YIFEAXLI.js";import{$ as O,$a as x,$b as M,Cb as n,Db as i,Eb as m,Ib as F,Mb as y,Nb as g,V as b,W as T,Wa as r,Zb as a,_b as d,bc as N,cc as B,dc as R,ec as j,fa as E,fc as W,ga as k,gb as u,ha as D,hb as L,ia as v,kb as A,mb as f,oc as P,pc as V,qc as $,sb as h,tb as l,ub as I,vb as z,yb as S}from"./chunk-MNLHA4RB.js";var ve=({dt:e})=>`
.p-progressspinner {
    position: relative;
    margin: 0 auto;
    width: 100px;
    height: 100px;
    display: inline-block;
}

.p-progressspinner::before {
    content: "";
    display: block;
    padding-top: 100%;
}

.p-progressspinner-spin {
    height: 100%;
    transform-origin: center center;
    width: 100%;
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    margin: auto;
    animation: p-progressspinner-rotate 2s linear infinite;
}

.p-progressspinner-circle {
    stroke-dasharray: 89, 200;
    stroke-dashoffset: 0;
    stroke: ${e("progressspinner.colorOne")};
    animation: p-progressspinner-dash 1.5s ease-in-out infinite, p-progressspinner-color 6s ease-in-out infinite;
    stroke-linecap: round;
}

@keyframes p-progressspinner-rotate {
    100% {
        transform: rotate(360deg);
    }
}
@keyframes p-progressspinner-dash {
    0% {
        stroke-dasharray: 1, 200;
        stroke-dashoffset: 0;
    }
    50% {
        stroke-dasharray: 89, 200;
        stroke-dashoffset: -35px;
    }
    100% {
        stroke-dasharray: 89, 200;
        stroke-dashoffset: -124px;
    }
}
@keyframes p-progressspinner-color {
    100%,
    0% {
        stroke: ${e("progressspinner.colorOne")};
    }
    40% {
        stroke: ${e("progressspinner.colorTwo")};
    }
    66% {
        stroke: ${e("progressspinner.colorThree")};
    }
    80%,
    90% {
        stroke: ${e("progressspinner.colorFour")};
    }
}
`,xe={root:"p-progressspinner",spin:"p-progressspinner-spin",circle:"p-progressspinner-circle"},fe=(()=>{class e extends q{name="progressspinner";theme=ve;classes=xe;static \u0275fac=(()=>{let t;return function(p){return(t||(t=v(e)))(p||e)}})();static \u0275prov=b({token:e,factory:e.\u0275fac})}return e})();var w=(()=>{class e extends ee{styleClass;style;strokeWidth="2";fill="none";animationDuration="2s";ariaLabel;_componentStyle=O(fe);static \u0275fac=(()=>{let t;return function(p){return(t||(t=v(e)))(p||e)}})();static \u0275cmp=u({type:e,selectors:[["p-progressSpinner"],["p-progress-spinner"],["p-progressspinner"]],inputs:{styleClass:"styleClass",style:"style",strokeWidth:"strokeWidth",fill:"fill",animationDuration:"animationDuration",ariaLabel:"ariaLabel"},features:[j([fe]),A],decls:3,vars:11,consts:[["role","progressbar",1,"p-progressspinner",3,"ngStyle","ngClass"],["viewBox","25 25 50 50",1,"p-progressspinner-spin"],["cx","50","cy","50","r","20","stroke-miterlimit","10",1,"p-progressspinner-circle"]],template:function(s,p){s&1&&(n(0,"div",0),D(),n(1,"svg",1),m(2,"circle",2),i()()),s&2&&(l("ngStyle",p.style)("ngClass",p.styleClass),h("aria-label",p.ariaLabel)("aria-busy",!0)("data-pc-name","progressspinner")("data-pc-section","root"),r(),I("animation-duration",p.animationDuration),h("data-pc-section","root"),r(),h("fill",p.fill)("stroke-width",p.strokeWidth))},dependencies:[C,H,U,_],encapsulation:2,changeDetection:0})}return e})(),he=(()=>{class e{static \u0275fac=function(s){return new(s||e)};static \u0275mod=L({type:e});static \u0275inj=T({imports:[w,_,_]})}return e})();var Me=()=>[25,50,100,200];function Pe(e,o){e&1&&(n(0,"div",7),m(1,"p-progressSpinner",8),n(2,"span",9),a(3,"Loading audit logs\u2026"),i()())}function we(e,o){e&1&&(n(0,"tr")(1,"th",17),a(2,"Date / Time"),i(),n(3,"th",18),a(4,"Method"),i(),n(5,"th"),a(6,"Path"),i(),n(7,"th"),a(8,"Action"),i(),n(9,"th",17),a(10,"User"),i(),n(11,"th",18),a(12,"Status"),i(),n(13,"th",19),a(14,"Duration"),i(),n(15,"th",20),a(16,"IP Address"),i()())}function be(e,o){if(e&1&&(n(0,"tr")(1,"td",21),a(2),P(3,"date"),i(),n(4,"td"),m(5,"p-tag",22),i(),n(6,"td")(7,"span",23),a(8),i()(),n(9,"td",24),a(10),i(),n(11,"td",25),a(12),i(),n(13,"td"),m(14,"p-tag",26),i(),n(15,"td",27),a(16),i(),n(17,"td",28),a(18),i()()),e&2){let t=o.$implicit,s=g(2);r(2),d($(3,13,t.createdDate,"dd MMM yy, HH:mm")),r(3),l("value",t.method)("severity",s.methodSeverity(t.method)),r(2),l("pTooltip",t.path),r(),d(t.path),r(2),d(t.action||"\u2014"),r(2),d(t.userName||"Anonymous"),r(2),l("value",t.statusCode.toString())("severity",s.statusSeverity(t.statusCode)),r(),z("slow",t.durationMs>1e3),r(),M(" ",t.durationMs," ms "),r(2),d(t.ipAddress||"\u2014")}}function Te(e,o){e&1&&(n(0,"p",32),a(1,"Try clearing your search term."),i())}function Oe(e,o){if(e&1&&(n(0,"tr")(1,"td",29)(2,"div",30),m(3,"i",31),n(4,"p"),a(5,"No audit log entries found."),i(),f(6,Te,2,0,"p",32),i()()()),e&2){let t=g(2);r(6),S(t.searchTerm?6:-1)}}function Ee(e,o){if(e&1){let t=F();n(0,"p-table",10),f(1,we,17,0,"ng-template",11)(2,be,19,16,"ng-template",12)(3,Oe,7,1,"ng-template",13),i(),n(4,"div",14)(5,"span",15),a(6),P(7,"number"),i(),n(8,"p-paginator",16),y("onPageChange",function(p){E(t);let c=g();return k(c.onPageChange(p))}),i()()}if(e&2){let t=g();l("value",t.logs),r(6),M("Total: ",V(7,6,t.totalRecords)," records"),r(2),l("rows",t.pageSize)("totalRecords",t.totalRecords)("rowsPerPageOptions",W(8,Me))("first",(t.page-1)*t.pageSize)}}var ye=class e{constructor(o,t){this.api=o;this.notify=t}api;notify;logs=[];totalRecords=0;loading=!1;page=1;pageSize=50;searchTerm="";searchDebounce=null;ngOnInit(){this.loadLogs()}loadLogs(){this.loading=!0,this.api.getAuditLogsPaged(this.page,this.pageSize,this.searchTerm||null).subscribe({next:o=>{this.logs=o.items,this.totalRecords=o.totalCount,this.loading=!1},error:o=>{this.loading=!1,this.notify.error(o.error?.message??"Failed to load audit logs.")}})}onSearchChange(){this.searchDebounce&&clearTimeout(this.searchDebounce),this.searchDebounce=setTimeout(()=>{this.page=1,this.loadLogs()},350)}onPageChange(o){this.page=(o.page??0)+1,this.pageSize=o.rows??this.pageSize,this.loadLogs()}refresh(){this.page=1,this.searchTerm="",this.loadLogs()}statusSeverity(o){return o>=200&&o<300?"success":o>=300&&o<400?"info":o>=400&&o<500?"warn":o>=500?"danger":"secondary"}methodSeverity(o){switch(o?.toUpperCase()){case"GET":return"info";case"POST":return"success";case"PUT":return"warn";case"PATCH":return"warn";case"DELETE":return"danger";default:return"secondary"}}static \u0275fac=function(t){return new(t||e)(x(ue),x(re))};static \u0275cmp=u({type:e,selectors:[["app-audit-logs"]],decls:11,vars:3,consts:[[1,"page-header"],[1,"header-actions"],[1,"p-input-icon-left","search-wrapper"],[1,"pi","pi-search"],["pInputText","","placeholder","Search by user, action, path\u2026",1,"search-input",3,"ngModelChange","ngModel"],["pButton","","type","button","icon","pi pi-refresh","label","Refresh",1,"p-button-outlined",3,"click","disabled"],[1,"card"],[1,"loading-wrapper"],["styleClass","w-8 h-8","strokeWidth","4"],[1,"loading-text"],["dataKey","id","responsiveLayout","scroll","styleClass","p-datatable-sm",3,"value"],["pTemplate","header"],["pTemplate","body"],["pTemplate","emptymessage"],[1,"paginator-row"],[1,"total-text","text-muted","text-sm"],[3,"onPageChange","rows","totalRecords","rowsPerPageOptions","first"],[2,"width","9rem"],[2,"width","6rem"],[2,"width","7rem"],[2,"width","8rem"],[1,"text-sm","text-muted","text-nowrap"],["styleClass","method-tag",3,"value","severity"],["tooltipPosition","top",1,"path-cell",3,"pTooltip"],[1,"action-cell"],[1,"text-sm","font-semibold"],[3,"value","severity"],[1,"text-sm","duration-cell"],[1,"text-sm","text-muted","mono"],["colspan","8",1,"empty-cell"],[1,"empty-state"],[1,"pi","pi-list","empty-icon"],[1,"text-muted","text-sm"]],template:function(t,s){t&1&&(n(0,"div",0)(1,"h1"),a(2,"Audit Logs"),i(),n(3,"div",1)(4,"span",2),m(5,"i",3),n(6,"input",4),R("ngModelChange",function(c){return B(s.searchTerm,c)||(s.searchTerm=c),c}),y("ngModelChange",function(){return s.onSearchChange()}),i()(),n(7,"button",5),y("click",function(){return s.refresh()}),i()()(),n(8,"div",6),f(9,Pe,4,0,"div",7)(10,Ee,9,9),i()),t&2&&(r(6),N("ngModel",s.searchTerm),r(),l("disabled",s.loading),r(2),S(s.loading?9:10))},dependencies:[C,K,G,Z,J,Q,X,ge,ce,Y,oe,ie,ne,te,se,ae,le,pe,de,me,he,w],styles:[".header-actions[_ngcontent-%COMP%]{display:flex;align-items:center;gap:.75rem}.search-wrapper[_ngcontent-%COMP%]{position:relative}.search-wrapper[_ngcontent-%COMP%]   .pi-search[_ngcontent-%COMP%]{position:absolute;left:.75rem;top:50%;transform:translateY(-50%);color:#94a3b8;z-index:1}.search-input[_ngcontent-%COMP%]{padding-left:2.25rem!important;width:20rem}.loading-wrapper[_ngcontent-%COMP%]{display:flex;flex-direction:column;align-items:center;padding:3rem;gap:1rem}.loading-wrapper[_ngcontent-%COMP%]   .loading-text[_ngcontent-%COMP%]{color:#64748b;font-size:.9rem}.text-sm[_ngcontent-%COMP%]{font-size:.8rem}.text-muted[_ngcontent-%COMP%]{color:#64748b}.font-semibold[_ngcontent-%COMP%]{font-weight:600}.text-nowrap[_ngcontent-%COMP%]{white-space:nowrap}.mono[_ngcontent-%COMP%]{font-family:Courier New,monospace}.path-cell[_ngcontent-%COMP%]{display:block;max-width:22rem;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;font-size:.82rem;color:#334155;font-family:monospace;cursor:default}.action-cell[_ngcontent-%COMP%]{font-size:.85rem;color:#475569}.duration-cell.slow[_ngcontent-%COMP%]{color:#ef4444;font-weight:600}[_nghost-%COMP%]     .method-tag .p-tag{font-family:monospace;font-size:.75rem;min-width:4rem;text-align:center}[_nghost-%COMP%]     .p-datatable-sm .p-datatable-tbody>tr>td{padding:.55rem .75rem}.empty-cell[_ngcontent-%COMP%]{padding:3rem 1rem!important}.empty-state[_ngcontent-%COMP%]{display:flex;flex-direction:column;align-items:center;gap:.75rem;color:#64748b}.empty-state[_ngcontent-%COMP%]   .empty-icon[_ngcontent-%COMP%]{font-size:2.5rem;color:#cbd5e1}.empty-state[_ngcontent-%COMP%]   p[_ngcontent-%COMP%]{margin:0;font-size:.95rem}.empty-state[_ngcontent-%COMP%]   .text-sm[_ngcontent-%COMP%]{font-size:.8rem}.paginator-row[_ngcontent-%COMP%]{display:flex;align-items:center;justify-content:space-between;margin-top:1rem}.paginator-row[_ngcontent-%COMP%]   .total-text[_ngcontent-%COMP%]{padding-left:.25rem}"]})};export{ye as AuditLogsComponent};
