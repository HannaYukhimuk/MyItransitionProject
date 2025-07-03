/**
 * Template View Manager - handles template viewing page functionality
 */
class TemplateViewManager {
  constructor() {
    this.initEventListeners();
    this.initTemplatePreview();
  }

  /**
   * Initialize all event listeners
   */
  initEventListeners() {
    this.initLikeButton();
    this.initTakeTemplateButton();
    this.initCommentActions();
  }

  /**
   * Initialize like button functionality
   */
  initLikeButton() {
    const likeForm = document.querySelector('.like-form');
    if (!likeForm) return;

    likeForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      
      try {
        const response = await fetch(likeForm.action, {
          method: 'POST',
          body: new FormData(likeForm),
          headers: {
            'X-Requested-With': 'XMLHttpRequest'
          }
        });

        if (!response.ok) throw new Error('Like action failed');

        const data = await response.json();
        this.updateLikeUI(data);
      } catch (error) {
        console.error('Like error:', error);
        alert('Failed to update like. Please try again.');
      }
    });
  }

  /**
   * Update like button UI
   */
  updateLikeUI(data) {
    const likeIcon = document.querySelector('.like-icon');
    const likeCount = document.querySelector('.like-count');
    
    likeIcon.className = data.isLiked ? 'fas fa-heart text-danger fs-4' : 'far fa-heart text-danger fs-4';
    likeCount.textContent = data.likesCount;
  }

  /**
   * Initialize take template button
   */
  initTakeTemplateButton() {
    const takeTemplateBtn = document.getElementById('takeTemplateBtn');
    if (!takeTemplateBtn) return;

    takeTemplateBtn.addEventListener('click', () => {
      const modal = new bootstrap.Modal(document.getElementById('takeTemplateModal'));
      modal.show();
    });
  }

  /**
   * Initialize template preview functionality
   */
  initTemplatePreview() {
    // Highlight required questions
    document.querySelectorAll('.question-preview').forEach(question => {
      if (question.dataset.required === 'true') {
        question.querySelector('.question-title').classList.add('text-primary', 'fw-bold');
      }
    });
  }

  /**
   * Initialize comment actions (replies, edits)
   */
  initCommentActions() {
    // Delegated event for reply buttons
    document.addEventListener('click', (e) => {
      if (e.target.closest('.reply-btn')) {
        this.toggleReplyForm(e.target.closest('.reply-btn'));
      }
      
      if (e.target.closest('.edit-comment-btn')) {
        this.showEditForm(e.target.closest('.edit-comment-btn'));
      }
    });
  }

  /**
   * Toggle reply form visibility
   */
  toggleReplyForm(button) {
    const commentId = button.dataset.commentId;
    const replyForm = document.querySelector(`.reply-form[data-comment-id="${commentId}"]`);
    replyForm.classList.toggle('d-none');
    
    if (!replyForm.classList.contains('d-none')) {
      replyForm.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
      replyForm.querySelector('textarea').focus();
    }
  }

  /**
   * Show comment edit form
   */
  showEditForm(button) {
    const commentId = button.dataset.commentId;
    const commentElement = document.querySelector(`.comment[data-comment-id="${commentId}"]`);
    
    commentElement.querySelector('.comment-text').classList.add('d-none');
    commentElement.querySelector('.edit-comment-form').classList.remove('d-none');
    commentElement.querySelector('.edit-comment-textarea').focus();
  }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => new TemplateViewManager());