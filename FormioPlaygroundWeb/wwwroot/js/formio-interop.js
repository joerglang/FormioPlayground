window.formioInterop = {
    _form: null,

    // JWT-Token für form.io-Authentifizierung setzen (z.B. nach Login)
    setToken: function (jwtToken) {
        localStorage.setItem('formioToken', jwtToken);
     },

    // Gemeinsame form.io-Optionen (Sprache / Übersetzungen)
    _options: {
        language: 'de',
        i18n: {
            de: {
                translation: {
                    // Wizard-Navigation
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
    },

    // Gemeinsame Event-Listener nach dem Form-Aufbau anhängen
    _attachListeners: function (form, dotNetRef) {
        form.on('submit', function (submission) {
            dotNetRef.invokeMethodAsync('OnFormSubmitted', JSON.stringify(submission.data));
        });
        form.on('submitDone', function (submission) {
            dotNetRef.invokeMethodAsync('OnSubmitDone', JSON.stringify(submission));
        });

        // Ortschaft-Dropdown automatisch öffnen nach PLZ-Lookup
        if (form.getComponent('plz') && form.getComponent('ortschaft')) {
            form.on('change', function (changed) {
                if (!changed.changed || changed.changed.component.key !== 'plz') return;
                var plz = (changed.changed.value || '').trim();
                if (plz.length < 4) return;
                setTimeout(function () {
                    var comp = form.getComponent('ortschaft');
                    if (comp && comp.choices && comp.choices._currentState.choices.length > 0) {
                        comp.choices.showDropdown();
                    }
                }, 400);
            });
        }
    },

    // Formular aus lokalem Schema-Objekt erstellen (lokale JSON-Formulare)
    createForm: function (elementId, schema, dotNetRef) {
        var el = document.getElementById(elementId);
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
        Formio.createForm(el, schema, window.formioInterop._options).then(function (form) {
            window.formioInterop._form = form;
            window.formioInterop._attachListeners(form, dotNetRef);
        });
    },

    // Formular direkt per URL vom form.io-Portal laden
    createFormByUrl: function (elementId, url, dotNetRef) {
        var el = document.getElementById(elementId);
        Formio.createForm(el, url, window.formioInterop._options).then(function (form) {
            window.formioInterop._form = form;
            window.formioInterop._attachListeners(form, dotNetRef);
        });
    },

    destroyForm: function () {
        if (window.formioInterop._form) {
            window.formioInterop._form.destroy();
            window.formioInterop._form = null;
        }
    }
};