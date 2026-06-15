<script setup lang="ts">
import RoleGate from '@/components/common/RoleGate.vue'
</script>

<template>
  <RoleGate :roles="['Admin', 'Sales', 'Marketing']">
    <div class="page">
      <h1>Chi tiết Sản phẩm</h1>
      <p>Giao diện đang được thiết kế lại theo phong cách Glassmorphism mới.</p>
    </div>
  </RoleGate>
</template>
