<script lang="ts" setup>
import { useCevex } from '@/composables/cevex';
import { ref } from 'vue';
import type { CevexInformation } from '@/models/admin/cevex';
import { Button, Column, DataTable, useDialog, useToast } from 'primevue';
import UserPeek from '@/components/UserPeek.vue';
import type { UserInfoMinimal } from '@/models/user/userInfoMinimal';
import CevexAttachDialog from '@/components/Admin/CevexAttachDialog.vue';

const cevex = useCevex();
const dialog = useDialog();
const toast = useToast();
const data = ref<CevexInformation | null>(null);

data.value = await cevex.getInformation();

function match(student: UserInfoMinimal) {
    dialog.open(CevexAttachDialog, {
        props: {
            header: 'Regelmäßigkeit bearbeiten',
            style: { width: '35rem' },
            modal: true,
        },
        data: {
            options: data.value.available ?? [],
            student,
        },
        onClose: async (result) => {
            if (!result?.data) return;
            await cevex.setMatch(student, result.data.result);
            toast.add({
                severity: 'success',
                summary: 'Zuweisung erfolgreich',
                life: 10000,
            });
            data.value = await cevex.getInformation();
        },
    });
}
</script>

<template>
    <h1>Cevex Nutzersynchronisierung</h1>
    <DataTable :value="data.matches ?? []">
        <Column header="Nutzer">
            <template #body="{ data }">
                <UserPeek :person="data.user" showGroup />
            </template>
        </Column>
        <Column header="Cevex">
            <template #body="{ data }">
                <template v-if="data.cevex !== null">
                    {{ data.cevex.firstName }} {{ data.cevex.lastName }}
                </template>
                <template v-else>
                    <Button
                        icon="pi pi-arrow-right"
                        label="Zuweisen"
                        severity="primary"
                        @click="match(data.user)"
                    />
                </template>
            </template>
        </Column>
    </DataTable>
</template>

<style scoped></style>
