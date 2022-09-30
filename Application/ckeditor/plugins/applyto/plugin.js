CKEDITOR.plugins.add('applyto',
    {
        init: function (editor) {
            var pluginName = 'applyto';
            editor.ui.addButton('applyto',
                {
                    label: 'Apply to',
                    command: 'OpenWindow',
                    icon: this.path + 'applyto.png'
                });
            // var cmd = editor.addCommand('OpenWindow', { exec: showMyDialog });
        }
    });