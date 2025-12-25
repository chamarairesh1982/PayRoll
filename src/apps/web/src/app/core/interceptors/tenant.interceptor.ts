import { HttpInterceptorFn } from '@angular/common/http';

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
    const storedUser = localStorage.getItem('currentUser');
    let token = '';

    if (storedUser) {
        try {
            const user = JSON.parse(storedUser);
            token = user.token;
        } catch (e) {
            console.error("Invalid user data in local storage", e);
        }
    }

    // Still attach X-Tenant-Id for now as fallback, but rely on Auth Header mainly if available
    const DEV_TENANT_ID = '00000000-0000-0000-0000-000000000001';
    const storedTenant = localStorage.getItem('tenantId') || DEV_TENANT_ID;

    let headers: any = {
        'X-Tenant-Id': storedTenant
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const authReq = req.clone({
        setHeaders: headers
    });

    return next(authReq);
};
