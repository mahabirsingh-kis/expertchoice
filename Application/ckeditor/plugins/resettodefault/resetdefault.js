
CKEDITOR.plugins.add('resettodefault',
    {
        init: function (editor) {
            var pluginName = 'resettodefault';
            editor.ui.addButton('resettodefault',
                {
                    label: 'Reset to default',
                    command: 'OpenWindow',
                    icon: this.path + 'Resettodefault.png'
                });
            // var cmd = editor.addCommand('OpenWindow', { exec: showMyDialog });
        }
    });
function showMyDialog(e) {
    $("#MainContent_variablesdivModal").modal('show');
}