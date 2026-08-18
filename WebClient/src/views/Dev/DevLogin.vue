<script lang="ts" setup>
import { mande } from 'mande';
import { UserInfoMinimal } from '@/models/user/user.ts';
import { formatStudent, formatTutor } from '@/helpers/formatters.ts';
import { useRouter } from 'vue-router';
import { useUser } from '@/stores/user.ts';
import { computed } from 'vue';

const router = useRouter();
const user = useUser();
const api = mande('/api/dev/users');
const users = await api.get<UserInfoMinimal[]>();

async function signInUser(userInfo: UserInfoMinimal) {
    await api.post({ value: userInfo.id });
    await user.update();
    await router.push('/');
}

const personen = computed<[string, UserInfoMinimal[]][]>(() => {
    const sorted: UserInfoMinimal[] =
        users.sort((a, b) => {
            const A = (formatTutor(a) || '').toLowerCase();
            const B = (formatTutor(b) || '').toLowerCase();
            return A < B ? -1 : A > B ? 1 : 0;
        }) ?? [];

    const grouped = sorted.reduce<Record<string, UserInfoMinimal[]>>((acc, p) => {
        const key = p.gruppe && p.gruppe.trim() !== '' ? p.gruppe : p.rolle;
        (acc[key] ??= []).push(p);
        return acc;
    }, {});

    const parseGroup = (str: string) => {
        const match = /^(\d+)(.*)$/i.exec(str);
        if (match) {
            return {
                num: parseInt(match[1], 10),
                suffix: match[2].trim().toLowerCase(),
                hasNum: true,
            };
        }
        return { num: null, suffix: str.toLowerCase(), hasNum: false };
    };

    return Object.entries(grouped).sort(([a], [b]) => {
        const pa = parseGroup(a);
        const pb = parseGroup(b);

        if (pa.hasNum && !pb.hasNum) return 1;
        if (!pa.hasNum && pb.hasNum) return -1;

        if (!pa.hasNum && !pb.hasNum) {
            return pa.suffix.localeCompare(pb.suffix, 'de', { sensitivity: 'base' });
        }

        if (pa.num !== pb.num) return (pa.num ?? 0) - (pb.num ?? 0);
        return pa.suffix.localeCompare(pb.suffix, 'de', { sensitivity: 'base' });
    });
});
</script>

<template>
    <h1>Dev Login</h1>
    <template v-for="[key, userList] in personen">
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
