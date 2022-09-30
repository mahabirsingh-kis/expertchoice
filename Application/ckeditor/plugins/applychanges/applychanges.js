
CKEDITOR.plugins.add('applychanges',
    {
        init: function (editor) {
            var pluginName = 'applychanges';
            editor.ui.addButton('applychanges',
                {
                    label: 'Apply Changes',
                    command: 'OpenWindow',
                    icon: this.path + 'applychanges.png'
                });
           // var cmd = editor.addCommand('OpenWindow', { exec: showMyDialog });
        }
    });
function showMyDialog(e) {
    $("#MainContent_variablesdivModal").modal('show');
    // window.open('/Default.aspx', 'MyWindow', 'width=800,height=700,scrollbars=no,scrolling=no,location=no,toolbar=no');
}