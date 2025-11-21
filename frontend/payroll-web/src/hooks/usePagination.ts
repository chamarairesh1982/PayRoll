import { useState } from "react";

const usePagination = (initialPage = 1) => {
  const [page, setPage] = useState(initialPage);
  const [totalPages, setTotalPages] = useState(1);
  return { page, totalPages, setPage, setTotalPages };
};

export default usePagination;
