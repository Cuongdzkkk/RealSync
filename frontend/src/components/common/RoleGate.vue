<script setup lang="ts">
import type { UserProfile } from '@/types/auth';
import { useAuthStore } from '@/stores/useAuthStore';

const props = defineProps<{
  roles: UserProfile['role'][];
}>();

const authStore = useAuthStore();
</script>

<template>
  <slot v-if="authStore.user && props.roles.includes(authStore.user.role)" />
  <slot v-else name="fallback" />
</template>
