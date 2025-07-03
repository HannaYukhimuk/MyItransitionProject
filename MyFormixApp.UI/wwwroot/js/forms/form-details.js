class FormDetailsEditor {
  constructor() {
    this.editBtn = document.getElementById('editBtn');
    this.cancelEdit = document.getElementById('cancelEdit');
    this.editControls = document.getElementById('editControls');
    this.answerFields = document.querySelectorAll('.answer-field');
    this.originalValues = {};
    
    if (this.editBtn) {
      this.init();
    }
  }

  init() {
    this.saveOriginalValues();
    this.setupEventListeners();
  }

  saveOriginalValues() {
    this.originalValues = {};
    this.answerFields.forEach(field => {
      const name = field.name;
      
      if (field.type === 'checkbox') {
        const checkboxes = document.querySelectorAll(`input[name="${name}"]`);
        this.originalValues[name] = Array.from(checkboxes)
          .filter(cb => cb.checked)
          .map(cb => cb.value)
          .join(',');
      } else if (field.type === 'radio') {
        const selected = document.querySelector(`input[name="${name}"]:checked`);
        this.originalValues[name] = selected ? selected.value : '';
      } else {
        this.originalValues[name] = field.value;
      }
    });
  }

  restoreOriginalValues() {
    this.answerFields.forEach(field => {
      if (field.type === 'checkbox') {
        const savedValues = this.originalValues[field.name]?.split(',') || [];
        if (savedValues.includes(field.value)) {
          field.checked = true;
        }
      } else if (field.type === 'radio') {
        field.checked = (field.value === this.originalValues[field.name]);
      } else {
        field.value = this.originalValues[field.name];
      }
    });
  }

  toggleEditMode(enable) {
    this.answerFields.forEach(field => field.disabled = !enable);
    this.editControls.classList.toggle('d-none', !enable);
    this.editBtn.classList.toggle('d-none', enable);
  }

  setupEventListeners() {
    this.editBtn.addEventListener('click', () => {
      this.saveOriginalValues();
      this.toggleEditMode(true);
    });

    this.cancelEdit.addEventListener('click', () => {
      this.restoreOriginalValues();
      this.toggleEditMode(false);
    });
  }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => new FormDetailsEditor());