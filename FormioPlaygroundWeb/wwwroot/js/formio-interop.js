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
        Formio.createForm(el, schema, {
            language: 'de',
            i18n: {
                de: {
                    translation: {
                        // Wizard-Navigation (form.io 4.x: lowercase, translation-Namespace)
                        'next': 'Weiter',
                        'previous': 'Zurück',
                        'cancel': 'Abbrechen',
                        'submit': 'Absenden',
                        // Zeichenzähler
                        '{{length}} characters remaining': 'Noch {{length}} Zeichen',
                        // Select / Choices.js
                        'Type to search': 'Tippen zum Suchen',
                        'No results found': 'Keine Ergebnisse gefunden',
                        'No choices to choose from': 'Keine Einträge vorhanden',
                        'Loading...': 'Wird geladen ...',
                        // Validierung
                        'required': 'ist ein Pflichtfeld',
                        'pattern': 'entspricht nicht dem geforderten Format',
                        'minLength': 'muss mindestens {{length}} Zeichen enthalten',
                        'maxLength': 'darf maximal {{length}} Zeichen enthalten',
                        'min': 'muss mindestens {{min}} sein',
                        'max': 'darf maximal {{max}} sein',
                        'invalid_email': 'ist keine gültige E-Mail-Adresse',
                        'invalid_date': 'ist kein gültiges Datum',
                        'Add Another': 'Weiteren Eintrag hinzufügen',
                        'Remove': 'Entfernen',
                        'Please fix the following errors before submitting': 'Bitte korrigieren Sie folgende Fehler vor dem Absenden'
                    }
                }
            }
        }).then(function (form) {
            window.formioInterop._form = form;
            form.on('submit', function (submission) {
                form.emit('submitDone', submission);
                dotNetRef.invokeMethodAsync('OnFormSubmitted', JSON.stringify(submission.data));
            });

            // Dropdown automatisch öffnen nachdem PLZ-Lookup Resultate geliefert hat.
            // Nur aktivieren wenn das Formular beide Felder enthält.
            if (form.getComponent('plz') && form.getComponent('ortschaft')) {
                form.on('change', function (changed) {
                    if (!changed.changed || changed.changed.component.key !== 'plz') return;
                    var plz = (changed.changed.value || '').trim();
                    if (plz.length < 4) return;
                    // Kurze Wartezeit bis der URL-Fetch abgeschlossen ist
                    setTimeout(function () {
                        var comp = form.getComponent('ortschaft');
                        if (comp && comp.choices && comp.choices._currentState.choices.length > 0) {
                            comp.choices.showDropdown();
                        }
                    }, 400);
                });
            }
        });
    },

    destroyForm: function () {
        if (window.formioInterop._form) {
            window.formioInterop._form.destroy();
            window.formioInterop._form = null;
        }
    }
};