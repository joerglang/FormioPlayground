// Feature: PLZ → Ortschaft Dropdown
// Aktiviert sich automatisch auf jedem Formular mit den Feldern "plz" und "ortschaft".
window.formioFeatures = window.formioFeatures || [];
window.formioFeatures.push(function (form) {
    if (!form.getComponent('plz') || !form.getComponent('ortschaft')) return;

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
});
