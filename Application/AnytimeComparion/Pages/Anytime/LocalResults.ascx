<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="LocalResults.ascx.vb" Inherits=".LocalResults" %>
<style>
    .a .actions {
        right: 50px !important;
        top: 0 !important;
    }

    .a ._actions {
        right: 25px !important;
        top: 0 !important;
    }

    /*.txtStepTask {
        font-size: 20px !important;
    }*/

    .table {
        --bs-table-bg: transparent;
        --bs-table-accent-bg: transparent;
        --bs-table-striped-color: #212529;
        --bs-table-active-color: #212529;
        --bs-table-active-bg: rgba(0, 0, 0, 0.1);
        --bs-table-hover-color: #212529;
        --bs-table-hover-bg: rgba(0, 0, 0, 0.075);
        width: 100%;
        margin-bottom: 1rem;
        color: #212529;
        vertical-align: top;
        border-color: #dee2e6;
    }

        .table td, .table th {
            font-size: 16px !important;
        }

    .header-padding .info_content_wrapper {
        justify-content: space-between;
        padding-right: 2em;
    }

    .header-padding .info_content_wrapper {
        justify-content: space-between !important;
        padding-right: 2em !important;
    }
</style>
<div id="divContent" style="padding-top: 0;" runat="server"></div>
