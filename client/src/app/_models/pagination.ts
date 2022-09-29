export interface Pagination{
    currentPage: number;
    itemsPerPages: number;
    totalItems: number;
    totalPages: number;
}

export class PaginatedResult<T>
{
    result: T;//our list of members are going to be stored in the result property
    pagination: Pagination;// and the pagination information is going to be stored in the pagination there.

}