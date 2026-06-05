import { ref } from 'vue';
import { defineStore } from 'pinia';

export interface Project {
  id: string;
  name: string;
  developer: string;
  location: string;
  type: string;
  status: 'upcoming' | 'selling' | 'delivered';
  priceRange: string;
  progress: number;
  totalUnits: string;
  aiInterest: number;
  imageUrl: string;
  createdAt: string;
}

export const useProjectStore = defineStore('project', () => {
  const items = ref<Project[]>([
    {
      id: 'p-001',
      name: 'Grand Marina Saigon',
      developer: 'Masterise Homes & Marriott International',
      location: 'Số 2 Tôn Đức Thắng, Phường Bến Nghé, Quận 1, TP.HCM',
      type: 'Căn hộ hàng hiệu, Officetel, Shophouse',
      status: 'selling',
      priceRange: 'Từ 350 - 450 triệu/m²',
      progress: 90,
      totalUnits: '4,200 căn',
      aiInterest: 98,
      imageUrl: 'https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=900&q=80',
      createdAt: '2026-01-15T08:00:00Z'
    },
    {
      id: 'p-002',
      name: 'The Metropole Thủ Thiêm',
      developer: 'SonKim Land & Quốc Lộc Phát',
      location: 'Khu chức năng số 1, Thủ Thiêm, TP. Thủ Đức, TP.HCM',
      type: 'Căn hộ cao cấp, Penthouse, Duplex',
      status: 'selling',
      priceRange: 'Từ 180 - 260 triệu/m²',
      progress: 85,
      totalUnits: '1,534 căn',
      aiInterest: 94,
      imageUrl: 'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&w=900&q=80',
      createdAt: '2026-02-10T09:30:00Z'
    },
    {
      id: 'p-003',
      name: 'Vinhomes Grand Park',
      developer: 'Vingroup & Mitsubishi Corporation',
      location: 'Phường Long Bình & Long Thạnh Mỹ, Quận 9, TP. Thủ Đức, TP.HCM',
      type: 'Căn hộ đại đô thị, Nhà phố, Biệt thự',
      status: 'delivered',
      priceRange: 'Từ 50 - 75 triệu/m²',
      progress: 100,
      totalUnits: '44,000 căn',
      aiInterest: 88,
      imageUrl: 'https://images.unsplash.com/photo-1600585154526-990dced4db0d?auto=format&fit=crop&w=900&q=80',
      createdAt: '2025-06-20T10:00:00Z'
    },
    {
      id: 'p-004',
      name: 'De La Sol',
      developer: 'CapitaLand Development',
      location: 'Số 1 Tôn Thất Thuyết, Phường 1, Quận 4, TP.HCM',
      type: 'Căn hộ hạng sang, Shophouse',
      status: 'upcoming',
      priceRange: 'Từ 110 - 150 triệu/m²',
      progress: 60,
      totalUnits: '870 căn',
      aiInterest: 79,
      imageUrl: 'https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?auto=format&fit=crop&w=900&q=80',
      createdAt: '2026-03-05T07:15:00Z'
    }
  ]);

  const loading = ref(false);

  const addProject = (project: Project) => {
    items.value.unshift(project);
  };

  const updateProject = (updatedProject: Project) => {
    const idx = items.value.findIndex(p => p.id === updatedProject.id);
    if (idx !== -1) {
      items.value[idx] = updatedProject;
    }
  };

  const deleteProject = (id: string) => {
    items.value = items.value.filter(p => p.id !== id);
  };

  return {
    items,
    loading,
    addProject,
    updateProject,
    deleteProject
  };
});
