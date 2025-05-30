/**
 * Company Selector Module
 * Handles Company selection in the user menu
 */
(function () {
    'use strict';

    // Reference to the shared constants
    const CONFIG = window.WORKSPACE_CONSTANTS || {
        STORAGE_KEY: 'selectedWorkspaceId',
        WORKSPACE_CHANGED: 'workspaceChanged',
        WORKSPACE_NAME: 'workspaceName'
    };

    /**
     * Initialize the Company selector functionality
     */
    function initWorkspaceSelector() {
        const workspaceSelector = $('#workspaceSelector');

        if (!workspaceSelector.length) return;

        // Load workspaces from API
        loadWorkspaces(workspaceSelector);

        // Set up change handler
        setupChangeHandler(workspaceSelector);

        // Show notification if Company was just changed
        showNotificationIfWorkspaceChanged();
    }

    function showWorkspaceName() {
        const workspaceSelector = document.getElementById('workspaceSelector');
        const selectedWorkspaceName = document.getElementById('selectedWorkspaceName');

        if (workspaceSelector && selectedWorkspaceName) {
            // Set initial value
            const selectedOption = workspaceSelector.options[workspaceSelector.selectedIndex];
            if (selectedOption) {
                selectedWorkspaceName.textContent = selectedOption.text;
            }

            // Listen for changes
            workspaceSelector.addEventListener('change', function () {
                const selectedOption = this.options[this.selectedIndex];
                if (selectedOption) {
                    selectedWorkspaceName.textContent = selectedOption.text;
                }
            });
        }
    }



    /**
     * Show notification if Company was changed before page reload
     */
    function showNotificationIfWorkspaceChanged() {
        if (localStorage.getItem(CONFIG.WORKSPACE_CHANGED) === 'true') {
            const workspaceName = localStorage.getItem(CONFIG.WORKSPACE_NAME);
            if (workspaceName) {
                // Use the localized format string
                const message = abp.localization.getResource('Cyberjuice')('Company Changed').replace('{0}', workspaceName);
                abp.notify.info(message);
            }
            // Clear the flags
            localStorage.removeItem(CONFIG.WORKSPACE_CHANGED);
            localStorage.removeItem(CONFIG.WORKSPACE_NAME);
        }
    }

    /**
     * Load workspaces from the API and populate the dropdown
     * @param {jQuery} selectorElement - The Company selector element
     */
    function loadWorkspaces(selectorElement) {
        cyberjuice.companies.company.getAll()
            .then(result => {
                if (result?.length) {
                    populateOptions(selectorElement, result);
                    restoreSavedSelection(selectorElement);
                    // Show Company name after workspaces are loaded
                    showWorkspaceName();
                }
            })
            .catch(error => {
                // Silently handle error - could add proper error handling if required
            });
    }

    /**
     * Populate dropdown options with Company data
     * @param {jQuery} selectorElement - The Company selector element
     * @param {Array} workspaces - Array of Company objects
     */
    function populateOptions(selectorElement, workspaces) {
        workspaces.forEach(Company => {
            selectorElement.append(
                $('<option></option>')
                    .val(Company.id)
                    .text(Company.name)
            );
        });
    }

    /**
     * Restore previously selected Company from localStorage
     * @param {jQuery} selectorElement - The Company selector element
     */
    function restoreSavedSelection(selectorElement) {
        const savedWorkspaceId = localStorage.getItem(CONFIG.STORAGE_KEY);
        if (savedWorkspaceId) {
            selectorElement.val(savedWorkspaceId);
        }
    }

    /**
     * Set up change event handler for Company selection
     * @param {jQuery} selectorElement - The Company selector element
     */
    function setupChangeHandler(selectorElement) {
        selectorElement.on('change', function () {
            const selectedWorkspaceId = $(this).val();
            if (selectedWorkspaceId) {
                const workspaceName = $(this).find('option:selected').text();

                // Store data for after reload
                localStorage.setItem(CONFIG.STORAGE_KEY, selectedWorkspaceId);
                localStorage.setItem(CONFIG.WORKSPACE_CHANGED, 'true');
                localStorage.setItem(CONFIG.WORKSPACE_NAME, workspaceName);

                // Reload page immediately
                location.reload();
            }
        });
    }


    // Start initialization process
    $(document).ready(initWorkspaceSelector);
})(); 
