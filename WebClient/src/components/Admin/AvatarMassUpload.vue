<script lang="ts" setup>
import { h, markRaw, onUnmounted, ref, useTemplateRef } from 'vue';
import { usePeople } from '@/stores/people.ts';
import { UserInfoMinimal } from '@/models/user/user.ts';
import { formatStudent } from '@/helpers/formatters.ts';
import { TableColumn } from '@nuxt/ui';
import UAvatar from '@nuxt/ui/components/Avatar.vue';
import PersonSelector from '@/components/PersonSelector.vue';

interface FileUpload {
    file: File;
    components: string[];
    userId?: string;
    filename: string;
    url: string;
}

const emit = defineEmits(['close']);

const fileInput = useTemplateRef<HTMLInputElement>('fileInput');
const objectUrls = [] as string[];
const people = usePeople();
const toast = useToast();
const files = ref<FileUpload[]>([]);

// Ugly copy so we can edit safely
const peopleAvailable = ([...(people.personen ?? [])] as UserInfoMinimal[]).map((p) => {
    return {
        lastNameComponents: splitString(p.nachname),
        firstNameComponents: splitString(p.vorname),
        original: p,
        name: formatStudent(p),
        id: p.id,
    };
});

onUnmounted(() => {
    for (const url of objectUrls) {
        URL.revokeObjectURL(url);
    }
});

function splitString(input: string): string[] {
    return input.toLowerCase().split(/[, \.\-_]+/);
}

function handleFileChange(event: Event) {
    const target = event.target as HTMLInputElement;
    if ((target.files?.length ?? 0) == 0) return;

    for (const file of target.files!) {
        markRaw(file);
        const filename = file.name;
        const components = splitString(filename);
        const url = URL.createObjectURL(file);
        objectUrls.push(url);
        const personId = findPerson(components);
        files.value.push({
            file,
            components,
            userId: personId,
            filename,
            url,
        });
    }
}

function findPerson(components: string[]) {
    return peopleAvailable.find((p) => {
        // last name must match
        if (!p.lastNameComponents.every((name) => components.includes(name))) return false;
        return p.firstNameComponents.some((name) => components.includes(name));
    })?.id;
}

const uploadsDone = ref(0);
const uploading = ref(false);

async function upload() {
    uploading.value = true;
    for (const file of files.value) {
        try {
            if (file.userId == undefined) continue;
            const formData = new FormData();
            formData.append('file', file.file);
            const response = await fetch(`/api/people/${file.userId}/avatar`, {
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
        } catch (error) {
            console.error(error);
            toast.add({
                title: 'Hochladen Fehlgeschlagen',
                description: 'Ein unbekannter Fehler ist aufgetreten.',
                color: 'error',
            });
        } finally {
            uploadsDone.value++;
        }
    }
    await people.updatePersonen(true);
    toast.add({
        title: 'Hochladen abgeschlossen',
    });
    emit('close');
}

const columns: TableColumn<FileUpload>[] = [
    {
        id: 'avatar',
        cell: ({ row }) => h(UAvatar, { src: row.original.url, size: 'xl', lazy: true }),
    },
    {
        header: 'Dateiname',
        accessorKey: 'filename',
    },
    {
        header: 'Person',
        cell: ({ row }) =>
            h(PersonSelector, {
                modelValue: row.original.userId,
                'onUpdate:modelValue': (value) => {
                    row.original.userId = value as string | undefined;
                },
                class: 'w-full',
            }),
    },
];
</script>

<template>
    <UModal
        :fullscreen="!uploading"
        :ui="{
            footer: 'justify-end',
        }"
        description="Laden Sie hier größere Mengen an Bildern hoch und ordnen Sie diesen Personen zu."
        title="Bilder hochladen"
    >
        <template #body>
            <div v-if="files.length == 0" class="flex items-center justify-center h-full">
                <UButton
                    icon="i-lucide-upload"
                    label="Dateien Hinzufügen"
                    size="xl"
                    @click="fileInput?.click()"
                />
            </div>
            <UTable v-else-if="!uploading" :columns="columns" :data="files" />
            <UProgress v-else v-model="uploadsDone" :max="files.length" size="xl" status />
        </template>
        <template v-if="!uploading" #footer>
            <input
                ref="fileInput"
                accept="image/jpeg,image/png,image/webp"
                class="hidden"
                multiple
                type="file"
                @change="handleFileChange"
            />
            <UButton
                icon="i-lucide-upload"
                label="Dateien Hinzufügen"
                @click="fileInput?.click()"
            />
            <UButton
                :disabled="files.length == 0"
                color="success"
                icon="i-lucide-check"
                label="Abschließen"
                @click="upload"
            />
        </template>
    </UModal>
</template>

<style scoped></style>
