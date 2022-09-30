<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="Survey.ascx.vb" Inherits=".Survey" %>
<div id="divContent" class="divHeader" runat="server">
</div>
<script>
    function clearInputs(inputType, typeId, name, index) {
        debugger
        if (inputType != undefined && inputType != null && inputType != '') {
            if (inputType == 'radio') {
                $("input[type='radio'][name='" + name + "']").prop("checked", false);
            }
            if (inputType == 'select') {
                $("select[name='" + name + "']").prop("selectedIndex", 0);
            }

            if ($("input[type='text'][name='" + name + "']").length > 0) {
                $("input[type='text'][name='" + name + "']").val('');
                if (typeId != undefined && typeId != null && typeId == '3')
                    $("input[type='text'][name='" + name + "']").prop('disabled', true);
            }
            change_respondentAnswer(index, '', typeId);
        }
    }
    function onlyNumbers(event) {
        debugger
        if (event.type == "paste") {
            var clipboardData = event.clipboardData || window.clipboardData;
            var pastedData = clipboardData.getData('Text');
            if (isNaN(pastedData)) {
                event.preventDefault();

            } else {
                return;
            }
        }
    }

    function process(input) {
        let value = input.value;
        let numbers = value.replace(/[^0-9]/g, "");
        input.value = numbers;
    }

    function otherDisable(name, val) {
        debugger
        if (val != undefined && val != null && val != '') {
            if (val == 'other') {
                //$("input[type='text'][name='" + name + "']").val('');
                $("input[type='text'][name='" + name + "']").attr('disabled', false);
            }
            else if (val != 'other') {
                //$("input[type='text'][name='" + name + "']").val('');
                $("input[type='text'][name='" + name + "']").attr('disabled', true);
            }
        }
    }
</script>
