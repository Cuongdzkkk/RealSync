import { reactive } from 'vue';

export const usePagination = (pageSize = 20) => {
  const pagination = reactive({
    page: 1,
    pageSize,
    totalCount: 0
  });

  const setTotal = (totalCount: number) => {
    pagination.totalCount = totalCount;
  };

  const resetPage = () => {
    pagination.page = 1;
  };

  return {
    pagination,
    setTotal,
    resetPage
  };
};
