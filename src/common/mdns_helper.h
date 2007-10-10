/***************************************************************************
 *  mdns_helper.h
 *
 *  Copyright (C) 2007 Calvin Gaisford <calvinrg@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */


#ifndef __MDNS_BROWSER_H__
#define __MDNS_BROWSER_H__

#ifdef __cplusplus
extern "C" {
#endif

#include <dns_sd.h>
#include <pthread.h>

struct mdns_data_struct
{
	pthread_t		serviceThread;
	DNSServiceRef	serviceRef;
};

DNSServiceErrorType mdns_browser_init(	struct mdns_data_struct **data, 
										DNSServiceBrowseReply callBack);
void mdns_browser_free(	struct mdns_data_struct **data);
DNSServiceErrorType mdns_browser_resolve(	
							uint32_t interface, 
							const char *name, 
							const char *type, 
							const char *domain,
							DNSServiceResolveReply callBack);

#ifdef __cplusplus
}	// extern "C"
#endif



#endif		//__MDNS_BROWSER_H__