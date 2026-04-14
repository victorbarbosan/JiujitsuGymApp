export const BELT_OPTIONS = ['White', 'Grey', 'Yellow', 'Orange', 'Green', 'Blue', 'Purple', 'Brown', 'Black'];
export const ROLE_OPTIONS = ['Member', 'Teacher', 'Admin'];

function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

export function openModal(component) {
    component.form = { firstName: '', lastName: '', email: '', phoneNumber: '', belt: 'White', password: '', role: 'Member' };
    component.formErrors = [];
    component.showModal = true;
}

export function closeModal(component) {
    component.showModal = false;
}

async function fetchUsers(skip, query) {
    const params = new URLSearchParams({ skip });
    if (query) params.set('query', query);
    const response = await fetch(`/Admin/GetUsers?${params}`);
    if (!response.ok) throw new Error('Failed to fetch');
    return response.json();
}

export async function loadMore(component) {
    if (component.isLoading) return;
    component.isLoading = true;
    try {
        const newUsers = await fetchUsers(component.skip ?? 0, component.searchQuery ?? '');
        component.users = [...(component.users || []), ...newUsers];
        component.skip = (component.skip ?? 0) + newUsers.length;
    } catch (error) {
        console.error('Member Load Error:', error);
        throw error;
    } finally {
        component.isLoading = false;
    }
}

export async function searchUsers(component, query) {
    if (component.isLoading) return;
    component.isLoading = true;
    try {
        const users = await fetchUsers(0, query);
        component.users = users;
        component.skip = users.length;
    } catch (error) {
        console.error('Search Error:', error);
        throw error;
    } finally {
        component.isLoading = false;
    }
}

export async function submitCreate(component, e, onSuccess) {
    e.preventDefault();
    if (component.isSubmitting) return;
    component.isSubmitting = true;
    component.formErrors = [];

    try {
        const response = await fetch('/Admin/CreateUser', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify(component.form)
        });

        if (response.ok) {
            const newUser = await response.json();
            if (onSuccess) onSuccess(newUser);
        } else {
            const data = await response.json();
            component.formErrors = data.errors ?? ['An unexpected error occurred.'];
        }
    } catch {
        component.formErrors = ['Network error. Please try again.'];
    } finally {
        component.isSubmitting = false;
    }
}