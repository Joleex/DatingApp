import { HttpClient, HttpParams } from "@angular/common/http";
import { map } from "rxjs/operators";
import { PaginatedResult } from "../_models/pagination";

export function getPaginatedResult<T>(url, params, http: HttpClient) {
    
    const paginatedResult: PaginatedResult<T>=new PaginatedResult<T>();
      return http.get<T>(url,{ observe: 'response', params }).pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') !== null) {
            /*And we're going to check to make sure this is not equal to null.
            And then what we'll do is we'll say, let's stop paginated results. */
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }
          return paginatedResult;
        })
      );
    }
  
  export function  getPaginationHeaders(pageNumber:number, pageSize: number){
      let params=new HttpParams();
  
        params=params.append('pageNumber', pageNumber.toString());
        params=params.append('pageSize',pageSize.toString());
        return params;
      
    }