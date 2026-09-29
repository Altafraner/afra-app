<script lang="ts" setup>
import { formatStudent } from '@/helpers/formatters';
import { UserInfoMinimal } from '@/models/user/user';
import { shallowRef, useTemplateRef } from 'vue';
import { mande, MandeError } from 'mande';

const props = defineProps<{
    user: UserInfoMinimal;
}>();

const toast = useToast();
const fileInput = useTemplateRef<HTMLInputElement>('fileInput');
const hasAvatar = shallowRef(props.user.hasAvatar);

function getAvatarLink(user: UserInfoMinimal, size: number | 'original' = 64) {
    return `/api/people/${user.id}/avatar?dimension=${size}`;
}

async function handleFileChange(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
        const file = target.files[0];
        const formData = new FormData();
        formData.append('file', file);
        try {
            const response = await fetch(`/api/people/${props.user.id}/avatar`, {
                method: 'POST',
                body: formData,
            });

            if (!response.ok) {
                toast.add({
                    title: 'Hochladen Fehlgeschlagen',
                    description: `Fehlercode: ${response.status}`,
                    color: 'error',
                });
                return;
            }

            hasAvatar.value = true;
        } catch (error) {
            console.error(error);
            toast.add({
                title: 'Hochladen Fehlgeschlagen',
                description: 'Ein unbekannter Fehler ist aufgetreten.',
                color: 'error',
            });
        }
    }
}

async function deleteAvatar() {
    try {
        const api = mande(`/api/people/${props.user.id}/avatar`);
        await api.delete();
        hasAvatar.value = false;
    } catch (error) {
        console.error(error);
        const mandeError = error as MandeError;
        toast.add({
            title: 'Löschen fehlgeschlagen',
            description: `Fehlercode: ${mandeError.response.status}`,
            color: 'error',
        });
    }
}
</script>

<template>
    <div class="flex flex-row gap-2">
        <UAvatar
            :alt="formatStudent(user)"
            :src="hasAvatar ? getAvatarLink(user) : ''"
            loading="lazy"
        />
        <input
            ref="fileInput"
            accept="image/jpeg,image/png,image/webp"
            class="hidden"
            type="file"
            @change="handleFileChange"
        />
        <UButton v-if="hasAvatar" icon="i-lucide-x" variant="subtle" @click="deleteAvatar" />
        <UButton v-else icon="i-lucide-upload" @click="fileInput?.click()" />
        <UButton
            :disabled="!hasAvatar"
            :to="getAvatarLink(user, 'original')"
            color="secondary"
            download
            external
            icon="i-lucide-download"
            target="_blank"
            variant="subtle"
        />
    </div>
</template>

<style scoped></style>
