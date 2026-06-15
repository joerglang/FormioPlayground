window.formioInterop = {
    _form: null,

    createForm: function (elementId, schema, dotNetRef) {
        var el = document.getElementById(elementId);
        // Submit-Button hinzufügen falls im Schema nicht vorhanden
        var hasSubmit = Array.isArray(schema.components) &&
            schema.components.some(function (c) { return c.type === 'button' && c.action === 'submit'; });
        if (!hasSubmit) {
            schema = Object.assign({}, schema, {
                components: (schema.components || []).concat([{
                    type: 'button', action: 'submit', label: 'Absenden',
                    key: 'submit', theme: 'primary', input: true
                }])
            });
        }
        Formio.createForm(el, schema).then(function (form) {
            window.formioInterop._form = form;
            form.on('submit', function (submission) {
                // form.io re-enables the submit button after submitDone
                form.emit('submitDone', submission);
                dotNetRef.invokeMethodAsync('OnFormSubmitted', JSON.stringify(submission.data));
            });
        });
    },

    destroyForm: function () {
        if (window.formioInterop._form) {
            window.formioInterop._form.destroy();
            window.formioInterop._form = null;
        }
    }
};
