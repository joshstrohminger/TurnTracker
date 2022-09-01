import { LuxonPipe } from './luxon.pipe';

describe('LuxonPipe', () => {
  it('create an instance', () => {
    const pipe = new LuxonPipe();
    expect(pipe).toBeTruthy();
  });
});
