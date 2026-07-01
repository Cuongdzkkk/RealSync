import { ref } from 'vue';
import { defineStore } from 'pinia';
import type { ConnectedAccount, ConnectedAccountCreateRequest, ConnectedAccountReconnectRequest, ConnectedAccountAuditLog } from '@/types/connectedAccount';
import { connectedAccountService } from '@/services/connectedAccountService';

export const useConnectedAccountStore = defineStore('connectedAccount', () => {
  // State
  const accounts = ref<ConnectedAccount[]>([]);
  const auditLogs = ref<ConnectedAccountAuditLog[]>([]);
  const loading = ref(false);
  const actionLoading = ref(false);

  // Actions

  /** Lấy danh sách các tài khoản liên kết */
  async function fetchAccounts() {
    loading.value = true;
    try {
      const res = await connectedAccountService.getAccounts();
      accounts.value = res.data ?? [];
    } finally {
      loading.value = false;
    }
  }

  /** Liên kết tài khoản mới */
  async function createAccount(request: ConnectedAccountCreateRequest): Promise<ConnectedAccount> {
    actionLoading.value = true;
    try {
      const res = await connectedAccountService.createAccount(request);
      const newAcc = res.data;
      accounts.value.unshift(newAcc);
      return newAcc;
    } finally {
      actionLoading.value = false;
    }
  }

  /** Kết nối lại tài khoản (cập nhật token mới) */
  async function reconnectAccount(id: string, request: ConnectedAccountReconnectRequest): Promise<ConnectedAccount> {
    actionLoading.value = true;
    try {
      const res = await connectedAccountService.reconnectAccount(id, request);
      const updatedAcc = res.data;
      const idx = accounts.value.findIndex(a => a.id === id);
      if (idx !== -1) {
        accounts.value[idx] = updatedAcc;
      }
      return updatedAcc;
    } finally {
      actionLoading.value = false;
    }
  }

  /** Kiểm tra trạng thái liên kết */
  async function checkAccountHealth(id: string): Promise<ConnectedAccount> {
    actionLoading.value = true;
    try {
      const res = await connectedAccountService.checkAccountHealth(id);
      const updatedAcc = res.data;
      const idx = accounts.value.findIndex(a => a.id === id);
      if (idx !== -1) {
        accounts.value[idx] = updatedAcc;
      }
      return updatedAcc;
    } finally {
      actionLoading.value = false;
    }
  }

  /** Xóa tài khoản liên kết */
  async function deleteAccount(id: string): Promise<void> {
    actionLoading.value = true;
    try {
      await connectedAccountService.deleteAccount(id);
      accounts.value = accounts.value.filter(a => a.id !== id);
    } finally {
      actionLoading.value = false;
    }
  }

  /** Lấy lịch sử log hoạt động của tài khoản */
  async function fetchAuditLogs(id: string) {
    loading.value = true;
    try {
      const res = await connectedAccountService.getAuditLogs(id);
      auditLogs.value = res.data ?? [];
    } finally {
      loading.value = false;
    }
  }

  return {
    accounts,
    auditLogs,
    loading,
    actionLoading,
    fetchAccounts,
    createAccount,
    reconnectAccount,
    checkAccountHealth,
    deleteAccount,
    fetchAuditLogs
  };
});
