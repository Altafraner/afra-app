<script lang="ts" setup>
import { mande } from 'mande';
import { UserInfoMinimal } from '@/models/user/user.ts';
import { formatStudent } from '@/helpers/formatters.ts';
import { useRouter } from 'vue-router';
import { useUser } from '@/stores/user.ts';

const router = useRouter();
const user = useUser();
const api = mande('/api/dev/users');
const users = await api.get<Record<string, UserInfoMinimal[]>>();

async function signInUser(userInfo: UserInfoMinimal) {
    await api.post({ value: userInfo.id });
    await user.update();
    await router.push('/');
}
</script>

<template>
    <h1>Dev Login</h1>
    <template v-for="(userList, key) in users">
        <h2>{{ key }}</h2>
        <div class="flex flex-col gap-1">
            <UButton
                v-for="user in userList"
                :key="user.id"
                :label="formatStudent(user)"
                color="neutral"
                variant="ghost"
                @click="signInUser(user)"
            />
        </div>
    </template>
</template>

<style scoped></style>
