/**
 * Form Utilities - helper functions for form handling
 */
class FormUtils {
  /**
   * Initialize form validation with Bootstrap 5 validation
   */
  static initBootstrapValidation() {
    // Fetch all forms we want to apply custom Bootstrap validation styles to
    const forms = document.querySelectorAll('.needs-validation');

    // Loop over them and prevent submission
    Array.from(forms).forEach(form => {
      form.addEventListener('submit', event => {
        if (!form.checkValidity()) {
          event.preventDefault();
          event.stopPropagation();
        }

        form.classList.add('was-validated');
      }, false);
    });
  }

  /**
   * Handle AJAX form submissions
   */
  static handleAjaxForm(formSelector, options = {}) {
    const form = document.querySelector(formSelector);
    if (!form) return;

    const {
      beforeSubmit = () => {},
      onSuccess = () => {},
      onError = () => {},
      onComplete = () => {}
    } = options;

    form.addEventListener('submit', async (e) => {
      e.preventDefault();
      
      const submitButton = form.querySelector('[type="submit"]');
      const originalText = submitButton?.innerHTML;
      const formData = new FormData(form);

      try {
        // Before submit callback
        beforeSubmit(form);

        // Disable submit button
        if (submitButton) {
          submitButton.disabled = true;
          if (options.loadingText) {
            submitButton.innerHTML = options.loadingText;
          }
        }

        // Add spinner
        const spinner = this.createSpinner();
        if (submitButton) {
          submitButton.prepend(spinner);
        }

        // Send request
        const response = await fetch(form.action, {
          method: form.method,
          body: formData,
          headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
          }
        });

        if (!response.ok) {
          throw new Error(await response.text());
        }

        const data = await response.json();

        // Success callback
        onSuccess(data, form);

      } catch (error) {
        console.error('Form submission error:', error);
        
        // Error callback
        onError(error, form);

        // Show error message
        if (options.errorElement) {
          const errorElement = document.querySelector(options.errorElement);
          if (errorElement) {
            errorElement.textContent = this.getErrorMessage(error);
            errorElement.classList.remove('d-none');
          }
        } else {
          alert(this.getErrorMessage(error));
        }
      } finally {
        // Complete callback
        onComplete(form);

        // Restore submit button
        if (submitButton) {
          submitButton.disabled = false;
          if (originalText) {
            submitButton.innerHTML = originalText;
          }
          const spinner = submitButton.querySelector('.spinner-border');
          if (spinner) spinner.remove();
        }
      }
    });
  }

  /**
   * Create loading spinner
   */
  static createSpinner() {
    const spinner = document.createElement('span');
    spinner.className = 'spinner-border spinner-border-sm me-2';
    spinner.setAttribute('role', 'status');
    spinner.setAttribute('aria-hidden', 'true');
    return spinner;
  }

  /**
   * Get user-friendly error message
   */
  static getErrorMessage(error) {
    if (error.message) {
      try {
        const errorObj = JSON.parse(error.message);
        return errorObj.message || errorObj.title || 'An error occurred';
      } catch {
        return error.message;
      }
    }
    return 'An unexpected error occurred';
  }

  /**
   * Initialize character counters for text inputs
   */
  static initCharacterCounters() {
    document.querySelectorAll('[data-maxlength]').forEach(element => {
      const maxLength = element.dataset.maxlength;
      const counterId = element.dataset.counterId || `${element.id}-counter`;
      const counterElement = document.getElementById(counterId) || 
        this.createCounterElement(element, counterId);

      element.addEventListener('input', () => {
        const remaining = maxLength - element.value.length;
        counterElement.textContent = `${remaining} characters remaining`;
        counterElement.classList.toggle('text-danger', remaining < 10);
      });

      // Trigger initial update
      element.dispatchEvent(new Event('input'));
    });
  }

  /**
   * Create counter element if it doesn't exist
   */
  static createCounterElement(inputElement, counterId) {
    const counter = document.createElement('div');
    counter.id = counterId;
    counter.className = 'form-text text-muted small text-end';
    inputElement.parentNode.appendChild(counter);
    return counter;
  }

  /**
   * Initialize file input previews
   */
  static initFilePreviews() {
    document.querySelectorAll('[data-file-preview]').forEach(input => {
      const previewId = input.dataset.filePreview;
      const previewElement = document.getElementById(previewId);
      if (!previewElement) return;

      input.addEventListener('change', () => {
        if (input.files && input.files[0]) {
          const reader = new FileReader();

          reader.onload = (e) => {
            if (input.files[0].type.startsWith('image/')) {
              previewElement.innerHTML = `<img src="${e.target.result}" class="img-thumbnail" style="max-height: 200px;">`;
            } else {
              previewElement.textContent = input.files[0].name;
            }
          };

          reader.readAsDataURL(input.files[0]);
        } else {
          previewElement.innerHTML = '';
        }
      });
    });
  }

  /**
   * Initialize dynamic form fields (add/remove)
   */
  static initDynamicFields(containerSelector = '.dynamic-form') {
    document.querySelectorAll(containerSelector).forEach(container => {
      // Add new field
      container.querySelector('.add-field')?.addEventListener('click', () => {
        const template = container.querySelector('.field-template');
        if (!template) return;

        const newField = template.content.cloneNode(true);
        const newFieldElement = newField.querySelector('.dynamic-field');
        if (!newFieldElement) return;

        // Update index
        const totalFields = container.querySelectorAll('.dynamic-field').length;
        newFieldElement.innerHTML = newFieldElement.innerHTML
          .replace(/\[0\]/g, `[${totalFields}]`)
          .replace(/_0_/g, `_${totalFields}_`);

        container.insertBefore(newField, template);
      });

      // Remove field (delegated event)
      container.addEventListener('click', (e) => {
        if (e.target.closest('.remove-field')) {
          const field = e.target.closest('.dynamic-field');
          if (field && container.querySelectorAll('.dynamic-field').length > 1) {
            field.remove();
          }
        }
      });
    });
  }

  /**
   * Initialize all form utilities
   */
  static initAll() {
    this.initBootstrapValidation();
    this.initCharacterCounters();
    this.initFilePreviews();
    this.initDynamicFields();
  }
}

// Initialize all form utilities when DOM is loaded
document.addEventListener('DOMContentLoaded', () => FormUtils.initAll());

// Export for module systems if needed
if (typeof module !== 'undefined' && module.exports) {
  module.exports = FormUtils;
}