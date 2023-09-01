#include <stdint.h>

#include <limine/limine.h>

static volatile struct limine_framebuffer_request fbr = {
    .id = LIMINE_FRAMEBUFFER_REQUEST,
    .revision = 0
};

void kmain()
{
    // Should set the pixel at 0,0 to 0xFFFFFF (white)
    uint32_t *address = fbr.response->framebuffers[0]->address;
    address[0] = 0xFFFFFF;

    // Lock in a while loop
    while (1)
        ;;
}
    