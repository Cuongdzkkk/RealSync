<script setup lang="ts">
import RoleGate from '@/components/common/RoleGate.vue'
</script>

<template>
  <RoleGate :roles="['Admin', 'Marketing', 'Data Analyst']">
    <div class="page">
      <h1>Máy thu thập dữ liệu (Crawler)</h1>
      <p>Giao diện đang được thiết kế lại theo phong cách Glassmorphism mới.</p>
    </div>
  </RoleGate>
</template>
