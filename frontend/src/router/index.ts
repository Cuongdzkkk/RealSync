import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';

import type { UserProfile } from '@/types/auth';
import { requireAuth, requireRole } from './guards';

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/auth/LoginView.vue'),
    meta: { title: 'Đăng nhập' }
  },
  {
    path: '/unauthorized',
    name: 'unauthorized',
    component: () => import('@/views/auth/UnauthorizedView.vue'),
    meta: { title: 'Không có quyền truy cập' }
  },
  {
    path: '/admin',
    component: () => import('@/components/layout/AdminLayout.vue'),
    beforeEnter: requireAuth,
    children: [
      {
        path: '',
        redirect: '/admin/dashboard'
      },
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('@/views/dashboard/DashboardView.vue'),
        meta: {
          title: 'Tổng quan',
          roles: ['Admin', 'Manager', 'Sales', 'Marketing', 'Data Analyst']
        }
      },
      {
        path: 'properties',
        name: 'properties',
        component: () => import('@/views/properties/PropertyListView.vue'),
        meta: {
          title: 'Sản phẩm',
          roles: ['Admin', 'Sales', 'Marketing']
        }
      },
      {
        path: 'properties/create',
        name: 'property-create',
        component: () => import('@/views/properties/PropertyFormView.vue'),
        meta: { title: 'Tạo bất động sản' }
      },
      {
        path: 'properties/:id/edit',
        name: 'property-edit',
        component: () => import('@/views/properties/PropertyFormView.vue'),
        meta: { title: 'Chỉnh sửa bất động sản' }
      },
      {
        path: 'property-categories',
        name: 'property-categories',
        component: () => import('@/views/properties/PropertyCategoryView.vue'),
        meta: { title: 'Danh mục bất động sản' }
      },
      {
        path: 'projects',
        name: 'projects',
        component: () => import('@/views/projects/ProjectListView.vue'),
        meta: {
          title: 'Dự án',
          roles: ['Admin', 'Marketing']
        }
      },
      {
        path: 'properties/:id',
        name: 'property-detail',
        component: () => import('@/views/properties/PropertyDetailView.vue'),
        meta: {
          title: 'Chi tiết sản phẩm',
          roles: ['Admin', 'Sales', 'Marketing']
        }
      },
      {
        path: 'leads',
        name: 'leads',
        component: () => import('@/views/leads/LeadListView.vue'),
        meta: {
          title: 'Khách hàng',
          roles: ['Admin', 'Manager', 'Sales']
        }
      },
      {
        path: 'crawlers',
        name: 'crawlers',
        component: () => import('@/views/crawlers/CrawlerView.vue'),
        meta: {
          title: 'Máy thu thập',
          roles: ['Admin', 'Marketing', 'Data Analyst']
        }
      },
      {
        path: 'ai-classification',
        name: 'ai-classification',
        component: () => import('@/views/ai/AiClassificationView.vue'),
        meta: {
          title: 'Phân loại AI',
          roles: ['Admin', 'Manager', 'Sales']
        }
      },
      {
        path: 'content-ai',
        name: 'content-ai',
        component: () => import('@/views/content/ContentAiView.vue'),
        meta: {
          title: 'Nội dung AI',
          roles: ['Admin', 'Sales', 'Marketing']
        }
      },
      {
        path: 'settings',
        name: 'settings',
        component: () => import('@/views/settings/SettingsView.vue'),
        meta: {
          title: 'Cài đặt',
          roles: ['Admin']
        }
      }
    ]
  }
];

// Augment vue-router
declare module 'vue-router' {
  interface RouteMeta {
    title?: string;
    roles?: UserProfile['role'][];
  }
}

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior: () => ({ top: 0 })
});

router.beforeEach(requireRole);

router.afterEach((to) => {
  document.title = `${String(to.meta.title ?? 'App')} | RealSync`;
});

export default router;
