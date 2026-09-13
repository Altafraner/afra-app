<script setup>
import { ref, onMounted } from 'vue';
import { mande } from 'mande';

import Grid from '@/components/Form/Grid.vue';
import GridEditRow from '@/components/Form/GridEditRow.vue';
import CreateKategorieForm from '@/Profundum/components/Forms/CreateKategorieForm.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';

const toast = useToast();
const { requireConfirm } = useConfirmPopover();
const overlay = useOverlay();
const api = mande('/api/profundum/management/kategorie');

const kategorien = ref([]);
const loading = ref(true);

async function load() {
    loading.value = true;
    kategorien.value = await api.get();
    loading.value = false;
}

async function createKategorie(data) {
    try {
        await api.post(data);
        toast.add({ color: 'success', title: 'Kategorie angelegt' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Kategorie nicht speichern',
        });
    }
}

const createDialog = overlay.create(CreateKategorieForm);

async function openCreateDialog() {
    const data = await createDialog.open();
    if (!data) return;
    await createKategorie(data);
}

async function updateKategorie(k) {
    try {
        await api.put(`/${k.id}`, {
            bezeichnung: k.bezeichnung,
            profilProfundum: k.profilProfundum,
        });

        toast.add({ color: 'success', title: 'Kategorie gespeichert' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Kategorie nicht speichern',
        });
    }
}

async function deleteKategorie(k) {
    if (
        !(await requireConfirm(
            'Möchten Sie diese Kategorie wirklich löschen?',
            'Kategorie löschen',
        ))
    )
        return;

    try {
        await api.delete(`/${k.id}`);
        toast.add({
            color: 'success',
            title: 'Gelöscht',
            description: 'Kategorie wurde entfernt',
        });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Kategorie nicht löschen (wird sie noch verwendet?)',
        });
    }
}

onMounted(load);
</script>

<template>
    <h2 class="mt-6">Kategorien</h2>
    <p class="text-sm text-muted">
        Kategorien mit aktiviertem "Profilprofundum" unterliegen den Profil-Regeln (Pflicht,
        Kategorie-Vielfalt, max. eine pro Einwahlzeitraum) - welche Klassenstufen und Halbjahre
        betroffen sind, wird global konfiguriert.
    </p>

    <template v-if="loading">
        <div>Lade …</div>
    </template>

    <template v-else>
        <Grid v-if="kategorien.length">
            <GridEditRow
                v-for="k in kategorien"
                :key="k.id"
                :canDelete="true"
                @update="updateKategorie(k)"
                @delete="deleteKategorie(k)"
            >
                <template #body>
                    {{ k.bezeichnung }}
                    <span v-if="k.profilProfundum" class="text-sm text-muted">
                        (Profilprofundum)</span
                    >
                </template>

                <template #edit>
                    <div class="flex flex-col gap-2 w-full">
                        <div>
                            <label class="block mb-1">Bezeichnung</label>
                            <UInput v-model="k.bezeichnung" class="w-full" maxlength="50" />
                        </div>
                        <USwitch v-model="k.profilProfundum" label="Profilprofundum" />
                    </div>
                </template>
            </GridEditRow>
        </Grid>

        <div v-else>Keine Kategorien vorhanden.</div>

        <UButton
            icon="i-lucide-plus"
            label="Neue Kategorie"
            class="mt-4"
            @click="openCreateDialog"
        />
    </template>
</template>

<style scoped></style>
