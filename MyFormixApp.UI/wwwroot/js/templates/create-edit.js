class TemplateFormManager {
  constructor() {
    this.questionCounter = 0;
    this.init();
  }

  init() {
    this.setupPublicToggle();
    this.setupAllowedUsers();
    this.setupTags();
    this.setupQuestions();
    this.setupFormSubmission();
    this.setupImageUploads();
  }

  setupPublicToggle() {
    const isPublicCheckbox = document.getElementById('IsPublic');
    if (!isPublicCheckbox) return;

    const toggleVisibility = () => {
      const allowedUsersSection = document.getElementById('allowedUsersSection');
      allowedUsersSection.style.display = isPublicCheckbox.checked ? 'none' : 'block';
    };

    isPublicCheckbox.addEventListener('change', toggleVisibility);
    toggleVisibility(); 
  }

  setupAllowedUsers() {
    const container = document.getElementById('allowedUsersContainer');
    if (!container) return;

    const input = document.getElementById('userEmailInput');
    if (!input) return;

    input.addEventListener('keydown', async (e) => {
      if (e.key !== 'Enter') return;
      e.preventDefault();
      
      const email = input.value.trim();
      if (!email) return;

      try {
        const userId = await this.fetchUserIdByEmail(email);
        if (userId && !this.userExists(userId)) {
          this.addAllowedUser(userId, email);
          input.value = '';
        }
      } catch (error) {
        console.error('Failed to add user:', error);
        alert('User not found or error occurred');
      }
    });

    container.addEventListener('click', (e) => {
      if (e.target.classList.contains('btn-close')) {
        e.target.closest('.badge').remove();
      }
    });
  }

  async fetchUserIdByEmail(email) {
    const response = await fetch(`/api/users/getIdByEmail?email=${encodeURIComponent(email)}`);
    if (!response.ok) throw new Error('User not found');
    return await response.text();
  }

  userExists(userId) {
    return Array.from(document.querySelectorAll('input[name="AllowedUserIds"]'))
      .some(input => input.value === userId);
  }

  addAllowedUser(userId, email) {
    const container = document.getElementById('allowedUsersContainer');
    const badge = document.createElement('span');
    badge.className = 'badge bg-secondary me-1 mb-1';
    badge.innerHTML = `
      ${email}
      <input type="hidden" name="AllowedUserIds" value="${userId}" />
      <button type="button" class="btn-close btn-close-white btn-sm ms-1" aria-label="Remove"></button>
    `;
    container.appendChild(badge);
  }

  setupTags() {
    const input = document.getElementById('tagInput');
    if (!input) return;

    const container = document.getElementById('tagsContainer');
    if (!container) return;

    input.addEventListener('keydown', (e) => {
      if (e.key !== 'Enter' && e.key !== ',') return;
      e.preventDefault();
      
      const tag = input.value.trim();
      if (!tag) return;

      if (!this.tagExists(tag)) {
        this.addTag(tag);
        input.value = '';
      }
    });

    container.addEventListener('click', (e) => {
      if (e.target.classList.contains('btn-close')) {
        e.target.closest('.badge').remove();
      }
    });
  }

  tagExists(tag) {
    return Array.from(document.querySelectorAll('input[name="Tags"]'))
      .some(input => input.value.toLowerCase() === tag.toLowerCase());
  }

  addTag(tag) {
    const container = document.getElementById('tagsContainer');
    const badge = document.createElement('span');
    badge.className = 'badge bg-primary me-1 mb-1';
    badge.innerHTML = `
      ${tag}
      <input type="hidden" name="Tags" value="${tag}" />
      <button type="button" class="btn-close btn-close-white btn-sm ms-1" aria-label="Remove"></button>
    `;
    container.appendChild(badge);
  }

  setupQuestions() {
    const addBtn = document.getElementById('addQuestionBtn');
    if (!addBtn) return;

    addBtn.addEventListener('click', () => this.addQuestion());

    document.querySelectorAll('.question-card').forEach(question => {
      this.setupQuestionEvents(question);
      this.questionCounter++;
    });
  }

  addQuestion() {
    const type = document.getElementById('questionTypeSelect').value;
    const questionId = `question_${this.questionCounter++}`;
    const html = this.generateQuestionHtml(type, questionId);
    
    document.getElementById('questionsContainer').insertAdjacentHTML('beforeend', html);
    const question = document.getElementById(questionId);
    
    this.setupQuestionEvents(question);
    this.setupImageUpload(question);
    question.scrollIntoView({ behavior: 'smooth' });
  }

  generateQuestionHtml(type, id) {
    const baseHtml = `
      <div class="card mb-3 question-card" id="${id}" data-type="${type}">
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-center mb-2">
            <h5 class="card-title">${this.getQuestionTypeName(type)}</h5>
            <button type="button" class="btn-close" aria-label="Remove question"></button>
          </div>
          <div class="mb-3">
            <input type="text" class="form-control question-title" placeholder="Enter question text" required>
          </div>
    `;

    let typeSpecificHtml = '';
    let optionsHtml = '';

    if (type !== 'text') {
      optionsHtml = `
        <div class="options-container mb-3">
          ${this.generateOptionHtml(type, 1)}
        </div>
        <button type="button" class="btn btn-sm btn-outline-primary add-option">+ Add Option</button>
      `;
    } else {
      typeSpecificHtml = `
        <div class="form-text text-muted mb-2">User will see a text input field</div>
        <div class="form-floating">
          <textarea class="form-control" placeholder="User answer will appear here" style="height: 100px" disabled></textarea>
          <label>User answer will appear here</label>
        </div>
      `;
    }

    const footerHtml = `
      <div class="question-image-upload-container mb-3"></div>
      <div class="form-check mt-3">
        <input class="form-check-input" type="checkbox" checked>
        <label class="form-check-label">Required</label>
      </div>
        </div>
      </div>
    `;

    return baseHtml + typeSpecificHtml + (optionsHtml || '') + footerHtml;
  }

  getQuestionTypeName(type) {
    const names = {
      text: 'Text Question',
      radio: 'Single Choice Question',
      checkbox: 'Multiple Choice Question',
      select: 'Dropdown Question'
    };
    return names[type] || 'Question';
  }

  generateOptionHtml(type, index) {
    let inputHtml = '';
    
    if (type === 'radio' || type === 'checkbox') {
      inputHtml = `
        <div class="input-group-text">
          <input class="form-check-input mt-0" type="${type}" disabled>
        </div>
      `;
    }

    return `
      <div class="input-group mb-2 option-item">
        ${inputHtml}
        <input type="text" class="form-control option-text" placeholder="Option ${index}" value="Option ${index}">
        <button class="btn btn-outline-danger remove-option" type="button">-</button>
      </div>
    `;
  }

  setupQuestionEvents(question) {
    question.querySelector('.btn-close').addEventListener('click', () => question.remove());

    const addOptionBtn = question.querySelector('.add-option');
    if (addOptionBtn) {
      addOptionBtn.addEventListener('click', () => {
        const optionsContainer = question.querySelector('.options-container');
        const optionCount = optionsContainer.querySelectorAll('.option-item').length + 1;
        optionsContainer.insertAdjacentHTML('beforeend', 
          this.generateOptionHtml(question.dataset.type, optionCount));
        
        optionsContainer.lastElementChild.querySelector('.remove-option')
          .addEventListener('click', function() {
            this.closest('.option-item').remove();
          });
      });
    }

    question.querySelectorAll('.remove-option').forEach(btn => {
      btn.addEventListener('click', function() {
        this.closest('.option-item').remove();
      });
    });
  }

  setupImageUpload(question) {
    const container = question.querySelector('.question-image-upload-container');
    if (!container) return;

    container.innerHTML = `
      <label class="form-label">Question Image</label>
      <div class="input-group mb-2">
        <input type="file" class="form-control question-image-input" accept="image/*">
        <button class="btn btn-outline-secondary upload-image-btn" type="button">Upload</button>
      </div>
      <div class="question-image-preview mt-2" style="display: none;">
        <img src="" class="img-thumbnail" style="max-height: 150px;">
        <input type="hidden" class="question-image-url" name="QuestionImages">
        <button type="button" class="btn btn-sm btn-outline-danger mt-2 remove-image-btn">Remove Image</button>
      </div>
    `;

    const uploadBtn = container.querySelector('.upload-image-btn');
    const imageInput = container.querySelector('.question-image-input');
    const preview = container.querySelector('.question-image-preview');
    const removeBtn = container.querySelector('.remove-image-btn');

    uploadBtn.addEventListener('click', async () => {
      const file = imageInput.files[0];
      if (!file) {
        alert('Please select an image file first');
        return;
      }

      try {
        uploadBtn.disabled = true;
        uploadBtn.textContent = 'Uploading...';

        const formData = new FormData();
        formData.append('file', file);

        const response = await fetch('/templates/upload-question-image', {
          method: 'POST',
          body: formData
        });

        if (!response.ok) {
          throw new Error(await response.text());
        }

        const { imageUrl } = await response.json();
        preview.querySelector('img').src = imageUrl;
        preview.querySelector('.question-image-url').value = imageUrl;
        preview.style.display = 'block';
        imageInput.value = '';
      } catch (error) {
        console.error('Upload failed:', error);
        alert(`Error uploading image: ${error.message}`);
      } finally {
        uploadBtn.disabled = false;
        uploadBtn.textContent = 'Upload';
      }
    });

    removeBtn.addEventListener('click', () => {
      preview.style.display = 'none';
      preview.querySelector('img').src = '';
      preview.querySelector('.question-image-url').value = '';
      imageInput.value = '';
    });
  }

  setupFormSubmission() {
    const form = document.getElementById('templateForm');
    if (!form) return;

    form.addEventListener('submit', (e) => {
      const questions = this.collectQuestionsData();
      const input = document.createElement('input');
      input.type = 'hidden';
      input.name = 'QuestionsData';
      input.value = JSON.stringify(questions);
      form.appendChild(input);
    });
  }

  collectQuestionsData() {
    return Array.from(document.querySelectorAll('.question-card')).map((card, index) => {
      const question = {
        Type: card.dataset.type,
        Title: card.querySelector('.question-title').value,
        IsRequired: card.querySelector('.form-check-input').checked,
        Position: index,
        Options: []
      };

      const imageUrl = card.querySelector('.question-image-url')?.value;
      if (imageUrl) question.ImageUrl = imageUrl;

      if (question.Type !== 'text') {
        card.querySelectorAll('.option-item').forEach((option, optIndex) => {
          question.Options.push({
            Text: option.querySelector('.option-text').value,
            Position: optIndex
          });
        });
      }

      return question;
    });
  }
}

document.addEventListener('DOMContentLoaded', () => new TemplateFormManager());