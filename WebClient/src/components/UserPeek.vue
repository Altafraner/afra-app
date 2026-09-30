<script lang="ts" setup>
import { formatStudent } from '@/helpers/formatters';
import type { UserInfoMinimal } from '@/models/user/user';
import UserSlideover from '@/components/UserSlideover.vue';
import { getAvatarLink } from '@/helpers/user.ts';

defineOptions({ name: 'UserPeek' });
const overlay = useOverlay();

const props = withDefaults(
    defineProps<{
        showGroup?: boolean;
        fullSize?: boolean;
        person: UserInfoMinimal;
        displayFunction?: (person: UserInfoMinimal) => string;
        hideAvatar?: boolean;
    }>(),
    {
        fullSize: false,
        showGroup: false,
        displayFunction: formatStudent,
    },
);

async function openSlideover() {
    const modal = overlay.create(UserSlideover);
    await modal.open({ user: props.person });
}
</script>

<template>
    <UButton
        :class="{
            'w-full': fullSize,
        }"
        class="py-1 font-semibold h-8 max-w-full"
        size="lg"
        v-bind="$attrs"
        variant="ghost"
        @click="openSlideover"
    >
        <span class="inline-flex justify-between items-center gap-2 w-full min-w-0">
            <span>
                <UAvatar
                    v-if="!hideAvatar"
                    :src="person.hasAvatar ? getAvatarLink(person.id, 64) : ''"
                    class="mr-3"
                    icon="i-lucide-user"
                    lazy
                />
                <span class="min-w-0 flex-1 truncate text-center">
                    {{ displayFunction(person) }}
                </span>
            </span>
            <UBadge v-if="person && showGroup" class="shrink-0" color="info" variant="soft">{{
                person.gruppe != '' ? person.gruppe : person.rolle
            }}</UBadge>
        </span>
    </UButton>
</template>
