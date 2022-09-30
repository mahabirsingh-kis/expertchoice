
CKEDITOR.plugins.add('customvariables',
    {
        init: function (editor) {
            var pluginName = 'customvariables';
            editor.ui.addButton('customvariables',
                {
                    label: 'Add Variable',
                    command: 'OpenWindow',
                    icon: this.path + 'variables.png'
                });
           // var cmd = editor.addCommand('OpenWindow', { exec: showMyDialog });
        }
    });
function showMyDialog(e) {
    $("#MainContent_variablesdivModal").modal('show');
}