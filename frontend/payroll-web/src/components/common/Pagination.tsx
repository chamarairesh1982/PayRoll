type PaginationProps = {
  page: number;
  totalPages: number;
  onChange: (page: number) => void;
};

const Pagination = ({ page, totalPages, onChange }: PaginationProps) => (
  <div className="pagination">
    <button disabled={page <= 1} onClick={() => onChange(page - 1)}>
      Previous
    </button>
    <span>
      Page {page} of {totalPages}
    </span>
    <button disabled={page >= totalPages} onClick={() => onChange(page + 1)}>
      Next
    </button>
  </div>
);

export default Pagination;
