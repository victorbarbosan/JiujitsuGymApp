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

export async function loadMore(component) {
    if (component.isLoading) return;
    component.isLoading = true;
    try {
        const skip = component.skip ?? 0;
        const response = await fetch(`/Admin/GetUsers?skip=${skip}`);
        if (!response.ok) throw new Error('Failed to fetch');
        const newUsers = await response.json();
        component.users = [...(component.users || []), ...newUsers];
        component.skip = skip + (Array.isArray(newUsers) ? newUsers.length : 0);
        return newUsers;
    } catch (error) {
        console.error('Member Load Error:', error);
        throw error;
    } finally {
        component.isLoading = false;
    }
}

export async function submitCreate(component, e) {
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
            component.users = [newUser, ...(component.users || [])];
            component.skip = (component.skip ?? 0) + 1;
            closeModal(component);
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