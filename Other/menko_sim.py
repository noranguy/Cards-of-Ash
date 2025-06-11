import random
from collections import Counter

class Card:
    def __init__(self, card_type: str, card_class: str):
        self.c_type = card_type
        self.c_class = card_class

    def __str__(self):
        return f'Card(c_type={self.c_type}, c_class={self.c_class})'
    
presets = {
    'default': {
        'starting_hand': [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ],
        'omamori': 'none'
    }
}

type_to_idx = {'light': 0, 'regular': 1, 'heavy': 2}
num_cards = 6
type_prob = [0.05, 0.5, 0.95]
ceramic_prob = 0.25

def draw_card(card, shown=True):
        width = 18
        top = "+" + "-" * (width - 2) + "+"
        empty = "|" + " " * (width - 2) + "|"
        text = f"{card.c_type} {card.c_class}" if shown else "X"
        centered = "|" + text.center(width - 2) + "|"
        return [top, empty, empty, centered, empty, empty, top]

def draw_cards(cards, shown=None):
    if shown is None:
        shown = [True] * len(cards)
        
    card_lines = [draw_card(card, s) for card, s in zip(cards, shown)]

    combined_lines = []
    for i in range(len(card_lines[0])):
        line = "   ".join(card[i] for card in card_lines)
        combined_lines.append(line)

    return "\n".join(combined_lines)

class Agent:
    def __init__(self):
        self.omamori = 'none'
        self.great_skill = 0.05
        self.good_skill = 1
        self.skill_passed = True
        self.hand = []
        return
    
    def show(self, type, idx):
        return
    
    def trade(self, card):
        return
    
    def add_card(self, card):
        return
    
    def skill_check(self, threshold=0.5):
        self.skill_passed = True
        if random.random() < self.great_skill:
            return 1.25
        elif random.random() < self.good_skill:
            return 1
        self.skill_passed = False
        return 0.25

    def move(self):
        return -1, None
    
    def backward(self, card: Card):
        return

# random
class Agent0(Agent):
    def __init__(self):
        self.hand = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ]
        random.shuffle(self.hand)

        self.omamori = 'none'
        self.table = [False] * num_cards
        self.table_idx = 0
        self.great_skill = 0.05
        self.good_skill = 0.78

    def move(self):
        self.table_idx = 0
        
        while self.table[self.table_idx]:
            self.table_idx = random.randint(0, 5)

        return self.hand.pop(), (self.table_idx,), (self.skill_check(),)
    
    def add_card(self, card):
        self.hand.append(card)

    def backward(self, cards: list[tuple[str, int]]):
        if cards[0][0] is not None:
            self.table[self.table_idx] = True

class Human(Agent):
    def __init__(self, starting_hand: list[Card], omamori: str):
        self.hand = starting_hand
        self.omamori = omamori
    
    def skill_check(self, threshold=0.5):
        return 1
    
    def add_card(self, card):
        self.hand.append(card)

    def move(self):
        print('Hand: ')
        print(draw_cards(self.hand))
        idx = int(input(f'Which card to use? (number 0-{len(self.hand)-1}) '))
        table_idx = int(input(f'Which card to throw at? (number 0-{num_cards-1}) '))
        return self.hand.pop(idx), (table_idx,), (self.skill_check(),)

    def backward(self, _: tuple[Card]):
        return

# single-target
class Agent1(Agent):
    def __init__(self):
        self.hand = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ]
        self.omamori = 'none'
        self.great_skill = 0.05
        self.good_skill = 0.75

        self.type_freq = [0, 0, 0]
        self.hand_freq = [3, 3, 3]

        self.rank = [1, 1, 1]
        mod = [(1-x) * 1/3 for x in type_prob]
        mod_sum = sum(mod)
        h, m, l = (x / mod_sum for x in mod)
        self.rank_mod = [
            [l, m, h],
            [h, l, m],
            [m, h, l]
        ]

        self.idx = 0
        self.order = list(range(num_cards))
        random.shuffle(self.order)

        self.last = None
    
    def add_card(self, card):
        self.hand.append(card)
        self.hand_freq[type_to_idx[card.c_type]] += 1
        
    def move(self):
        self.hand.sort(key = lambda x: (self.rank[type_to_idx[x.c_type]], self.hand_freq[type_to_idx[x.c_type]]))
        check = self.skill_check()

        self.last = self.hand[-1].c_type
        self.hand_freq[type_to_idx[self.last]] -= 1
        return self.hand.pop(), (self.order[self.idx],), (check,)
    
    def backward(self, cards: list[tuple[str, int]]):
        if cards[0][0] is not None and self.idx < 5:
            self.type_freq[type_to_idx[cards[0][0]]] += 1
            self.idx += 1
            self.rank = [
                (2 - min(2, self.type_freq[2])) / (6 - self.idx),
                (2 - min(2, self.type_freq[0])) / (6 - self.idx),
                (2 - min(2, self.type_freq[1])) / (6 - self.idx),
            ]
        elif self.skill_passed:
            self.rank = [a * b for a, b in zip(self.rank, self.rank_mod[type_to_idx[self.last]])]

# vision, assume card types after a failed flip on each card on table
class Agent2(Agent):
    def __init__(self):
        self.hand = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ]
        self.omamori = 'vision'
        self.great_skill = 0.05
        self.good_skill = 0.77

        self.hand_freq = [3, 3, 3]
        self.type_freq = [0, 0, 0]

        self.known = [-1] * 6
        self.assume = [-1] * 6
        self.flipped = [False] * 6
        self.order = list(range(6))

        self.search_phase = True

        self.idx = 0
        self.last = 0
    
    def add_card(self, card):
        self.hand.append(card)
        self.hand_freq[type_to_idx[card.c_type]] += 1

    def show(self, type, idx):
        self.known[idx] = self.assume[idx] = type_to_idx[type]
        self.type_freq[type_to_idx[type]] += 1
        while self.idx < 6 and self.known[self.idx] != -1: self.idx += 1
    
    def fix_assumption(self, type):
        idx = next((i for i in range(6) if self.known[i] == -1 and self.assume[i] == type))
        prev_type = (type + 2) % 3
        next_type = (type + 1) % 3
        if self.type_freq[prev_type] == 2:
            if sum(1 for i in self.known if i == prev_type) == 2:
                self.assume[idx] = next_type
                self.type_freq[type] -= 1
                self.type_freq[next_type] += 1
            else:
                idx2 = next((i for i in range(6) if self.known[i] == -1 and self.assume[i] == prev_type))
                self.assume[idx2] = next_type
                self.assume[idx] = prev_type
                self.type_freq[type] -= 1
                self.type_freq[prev_type] += 1
        else:
            self.assume[idx] = prev_type
            self.type_freq[type] -= 1
            self.type_freq[prev_type] += 1

    def move(self):
        if self.search_phase:
            self.hand.sort(key = lambda x: {1: 0, 2: 1, 0: 2}[self.type_freq[(type_to_idx[x.c_type] + 1) % 3]])
            self.last = self.hand[-1].c_type
            self.hand_freq[type_to_idx[self.hand[-1].c_type]] -= 1
            return self.hand.pop(), (self.idx,), (self.skill_check(),)
        else:
            self.order.sort(key=lambda x: (
                not self.flipped[x],
                self.known[x] != -1,
                self.hand_freq[(self.assume[x] + 1) % 3],
                self.hand_freq[self.assume[x]]
            ))
            order = self.order[-1]
            assume = self.assume[order]
            priority = [(assume + 1) % 3, assume, (assume + 2) % 3]
            self.hand.sort(key=lambda x: priority.index(type_to_idx[x.c_type]), reverse=True)
            self.hand_freq[type_to_idx[self.hand[-1].c_type]] -= 1
            return self.hand.pop(), (order,), (self.skill_check(),)
 
    def backward(self, cards: list[tuple[str, int]]):
        if cards[0][0] is not None:
            card, idx = cards[0]
            self.flipped[idx] = True
            self.assume[idx] = self.known[idx] = type_to_idx[card]
        elif self.search_phase and self.skill_passed:
            self.assume[self.idx] = (type_to_idx[self.last] + 1) % 3

        if self.search_phase and self.skill_passed:
            self.type_freq[self.assume[self.idx]] += 1
            if self.type_freq[self.assume[self.idx]] > 2:
                self.fix_assumption(self.assume[self.idx])
            if self.type_freq.count(2) == 2:
                self.search_phase = False
                last = next(i for i, val in enumerate(self.type_freq) if val != 2)
                self.assume = [last if x == -1 else x for x in self.assume]

        if self.search_phase and (self.skill_passed or cards[0][0] is not None):
            self.idx += 1
            while self.idx < 6 and self.known[self.idx] != -1:
                self.idx += 1
            if self.idx == 6:
                self.search_phase = False

# trade, trade its worst card with the opponent and rank cards to throw at and throw with
class Agent3(Agent):
    def __init__(self):
        self.hand = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
        ]
        self.omamori = 'trade'
        self.great_skill = 0.05
        self.good_skill = 0.85

        self.type_freq = [0, 0, 0]
        self.hand_freq = [3, 3, 3]

        self.ranks = [[1/3] * 3] * 6
        self.orders = [list(range(6)) for _ in range(3)]
        h, m, l = [(1-x) * 1/3 for x in type_prob]
        self.rank_mod = [
            [l, m, h],
            [h, l, m],
            [m, h, l]
        ]
        self.flipped = [False] * 6

        self.idx = 0
        self.count = 0
        self.order = 0
        self.last = -1
    
    def trade(self, card):
        self.hand.sort(key = lambda x: sum(a[type_to_idx[x.c_type]] for a in self.ranks), reverse=True)
        ret = self.hand.pop()
        self.hand.append(card)
        self.hand_freq[type_to_idx[card.c_type]] += 1
        self.hand_freq[type_to_idx[ret.c_type]] -= 1
        return ret
    
    def add_card(self, card):
        self.hand.append(card)
        self.hand_freq[type_to_idx[card.c_type]] += 1

    def move(self):
        for idx, row in enumerate(self.orders):
            row.sort(key = lambda x: (not self.flipped[x], self.ranks[x][idx]), reverse=True)

        self.hand.sort(key = lambda x: (
            (self.ranks[self.orders[w := type_to_idx[x.c_type]][0]][w],
            self.hand_freq[type_to_idx[x.c_type]])
        ))
        
        self.last = type_to_idx[self.hand[-1].c_type]
        self.hand_freq[self.last] -= 1
        self.order = self.orders[self.last][0]
        return self.hand.pop(), (self.order,), (self.skill_check(),)
    
    def backward(self, cards: list[tuple[str, int]]):
        old_prob = [
            (2 - min(2, self.type_freq[2])) / (6 - self.count),
            (2 - min(2, self.type_freq[0])) / (6 - self.count),
            (2 - min(2, self.type_freq[1])) / (6 - self.count),
        ]
        for card, idx in cards:
            if card is not None:
                self.type_freq[type_to_idx[card]] += 1
                if not self.flipped[idx]:
                    self.count += 1
                self.flipped[idx] = True
        
        if self.count >= 6: return
        
        new_prob = [
            (2 - min(2, self.type_freq[2])) / (6 - self.count),
            (2 - min(2, self.type_freq[0])) / (6 - self.count),
            (2 - min(2, self.type_freq[1])) / (6 - self.count),
        ]

        self.ranks = [[(row[x] / old_prob[x] * new_prob[x] if old_prob[x] != 0 else 0) for x in range(3)] for row in self.ranks]

        for card, idx in cards:
            if card is not None:
                self.ranks[idx] = [-1e5, -1e5, -1e5]
            elif self.skill_passed:
                self.ranks[idx] = [a * b for a, b in zip(self.ranks[idx], self.rank_mod[self.last])]

        for i, row in enumerate(self.ranks):
            whole = sum(row)
            if whole == 0:
                self.ranks[i] = [-1e5, -1e5, -1e5]
            else:
                self.ranks[i] = [x / whole for x in row]

        self.idx += 1

# ceramic, use all ceramics first for information and then rank cards to throw at and throw with
class Agent4(Agent):
    def __init__(self):
        self.hand = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='ceramic'),
            Card(card_type='regular', card_class='ceramic'),
            Card(card_type='heavy', card_class='ceramic'),
        ]
        self.omamori = 'none'
        self.great_skill = 0.05
        self.good_skill = 0.87

        self.type_freq = [0, 0, 0]
        self.hand_freq = [3, 3, 3]

        self.ranks = [[1/3] * 3] * 6
        self.orders = [list(range(6)) for _ in range(3)]
        h, m, l = [(1-x) * 1/3 for x in type_prob]
        self.rank_mod = [
            [l, m, h],
            [h, l, m],
            [m, h, l]
        ]
        h2, m2, l2 = [(1-x * ceramic_prob) * 1/3 for x in type_prob]
        self.half_rank_mod = [
            [l2, m2, h2],
            [h2, l2, m2],
            [m2, h2, l2]
        ]
        self.flipped = [False] * 6

        self.idx = 0
        self.count = 0
        self.order = 0
        self.last = -1
    
    def add_card(self, card):
        self.hand.append(card)
        self.hand_freq[type_to_idx[card.c_type]] += 1

    def move(self):
        if self.idx == 0:
            self.last = type_to_idx[self.hand[-1].c_type]
            return self.hand.pop(), (1,), (self.skill_check(),)
        if self.idx == 1:
            self.last = type_to_idx[self.hand[-1].c_type]
            return self.hand.pop(), (4,), (self.skill_check(),)
        if self.idx == 2:
            for idx, row in enumerate(self.orders):
                row.sort(key = lambda x: (not self.flipped[x], self.ranks[x][idx] + (0 if x == 0 else ceramic_prob * self.ranks[x-1][idx]) + (0 if x == 5 else ceramic_prob * self.ranks[x+1][idx])), reverse=True)
            
            self.last = type_to_idx[self.hand[-1].c_type]
            self.order = self.orders[self.last][0]
            return self.hand.pop(), (self.order,), (self.skill_check(),)

        for idx, row in enumerate(self.orders):
            row.sort(key = lambda x: (not self.flipped[x], self.ranks[x][idx]), reverse=True)

        self.hand.sort(key = lambda x: (
            (self.ranks[self.orders[w := type_to_idx[x.c_type]][0]][w],
            self.hand_freq[type_to_idx[x.c_type]])
        ))
        
        self.hand_freq[type_to_idx[self.hand[-1].c_type]] -= 1

        self.last = type_to_idx[self.hand[-1].c_type]
        self.order = self.orders[self.last][0]
        return self.hand.pop(), (self.order,), (self.skill_check(),)
    
    def backward(self, cards: list[tuple[str, int]]):
        old_prob = [
            (2 - min(2, self.type_freq[2])) / (6 - self.count),
            (2 - min(2, self.type_freq[0])) / (6 - self.count),
            (2 - min(2, self.type_freq[1])) / (6 - self.count),
        ]
        for card, idx in cards:
            if card is not None:
                self.type_freq[type_to_idx[card]] += 1
                if not self.flipped[idx]:
                    self.count += 1
                self.flipped[idx] = True
        
        if self.count >= 6: return
        
        new_prob = [
            (2 - min(2, self.type_freq[2])) / (6 - self.count),
            (2 - min(2, self.type_freq[0])) / (6 - self.count),
            (2 - min(2, self.type_freq[1])) / (6 - self.count),
        ]

        self.ranks = [[(row[x] / old_prob[x] * new_prob[x] if old_prob[x] != 0 else 0) for x in range(3)] for row in self.ranks]

        for card, idx in cards:
            if card is not None:
                self.ranks[idx] = [-1e5, -1e5, -1e5]
            elif self.skill_passed:
                if idx == self.order:
                    self.ranks[idx] = [a * b for a, b in zip(self.ranks[idx], self.rank_mod[self.last])]
                else:
                    self.ranks[idx] = [a * b for a, b in zip(self.ranks[idx], self.half_rank_mod[self.last])]

        for i, row in enumerate(self.ranks):
            whole = sum(row)
            if whole == 0:
                self.ranks[i] = [-1e5, -1e5, -1e5]
            else:
                self.ranks[i] = [x / whole for x in row]

        self.idx += 1

# bouncy, use all bouncy first and rank cards to throw at and throw with
class Agent5(Agent):
    def __init__(self):
        self.hand = [
            Card(card_type='light', card_class='bouncy'),
            Card(card_type='regular', card_class='bouncy'),
            Card(card_type='heavy', card_class='bouncy'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
        ]
        self.omamori = 'none'
        self.great_skill = 0.05
        self.good_skill = 0.747

        self.type_freq = [0, 0, 0]
        self.hand_freq = [3, 3, 3]

        self.ranks = [[1/3] * 3] * 6
        self.orders = [list(range(6)) for _ in range(3)]
        mod = [(1-x) * 1/3 for x in type_prob]
        mod_sum = sum(mod)
        self.flipped = [False] * 6
        h, m, l = (x / mod_sum for x in mod)
        self.rank_mod = [
            [l, m, h],
            [h, l, m],
            [m, h, l]
        ]

        self.count = 0
    
    def add_card(self, card):
        self.hand.append(card)
        self.hand_freq[type_to_idx[card.c_type]] += 1

    def move(self):
        for idx, row in enumerate(self.orders):
            row.sort(key = lambda x: (self.flipped[x], -self.ranks[x][idx]))

        self.hand.sort(key = lambda x: (
            x.c_class == 'bouncy',
            (self.ranks[self.orders[w := type_to_idx[x.c_type]][0]][w]
            if x.c_class == 'basic' else
            (self.ranks[self.orders[w := type_to_idx[x.c_type]][0]][w] + self.ranks[self.orders[w][1]][w]),
            self.hand_freq[type_to_idx[x.c_type]])
        ))
        
        self.last = self.hand[-1].c_type
        self.hand_freq[type_to_idx[self.last]] -= 1

        if self.hand[-1].c_class == 'bouncy':
            order1 = self.orders[type_to_idx[self.last]][0]
            order2 = self.orders[type_to_idx[self.last]][1]
            return self.hand.pop(), (order1, order2), (self.skill_check(), self.skill_check())
        else:
            order = self.orders[type_to_idx[self.last]][0]
            return self.hand.pop(), (order,), (self.skill_check(),)
    
    def backward(self, cards: list[tuple[str, int]]):
        old_prob = [
            (2 - min(2, self.type_freq[2])) / (6 - self.count),
            (2 - min(2, self.type_freq[0])) / (6 - self.count),
            (2 - min(2, self.type_freq[1])) / (6 - self.count),
        ]
        for card, idx in cards:
            if card is not None:
                self.type_freq[type_to_idx[card]] += 1
                if not self.flipped[idx]:
                    self.count += 1
                self.flipped[idx] = True
        
        if self.count == 6: return
        
        new_prob = [
            (2 - min(2, self.type_freq[2])) / (6 - self.count),
            (2 - min(2, self.type_freq[0])) / (6 - self.count),
            (2 - min(2, self.type_freq[1])) / (6 - self.count),
        ]

        self.ranks = [[(row[x] / old_prob[x] * new_prob[x] if old_prob[x] != 0 else 0) for x in range(3)] for row in self.ranks]

        for card, idx in cards:
            if card is not None:
                self.ranks[idx] = [-1e5, -1e5, -1e5]
            elif self.skill_passed:
                self.ranks[idx] = [a * b for a, b in zip(self.ranks[idx], self.rank_mod[type_to_idx[self.last]])]

        for i, row in enumerate(self.ranks):
            whole = sum(row)
            if whole == 0:
                self.ranks[i] = [-1e5, -1e5, -1e5]
            else:
                self.ranks[i] = [x / whole for x in row]

class Agent6(Agent):
    def __init__(self):
        self.hand = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
        ]
        self.omamori = 'none'
        self.great_skill = 1
        self.good_skill = 1

        self.type_freq = [0, 0, 0]
        self.hand_freq = [3, 3, 3]

        self.ranks = [[1/3] * 3] * 6
        self.orders = [list(range(6)) for _ in range(3)]
        mod = [(1-x) * 1/3 for x in type_prob]
        mod_sum = sum(mod)
        self.flipped = [False] * 6
        h, m, l = (x / mod_sum for x in mod)
        self.rank_mod = [
            [l, m, h],
            [h, l, m],
            [m, h, l]
        ]

        self.count = 0
    
    def add_card(self, card):
        self.hand.append(card)
        self.hand_freq[type_to_idx[card.c_type]] += 1

    def move(self):
        for idx, row in enumerate(self.orders):
            row.sort(key = lambda x: (self.flipped[x], -self.ranks[x][idx]))

        self.hand.sort(key = lambda x: (
            x.c_class == 'bouncy',
            (self.ranks[self.orders[w := type_to_idx[x.c_type]][0]][w]
            if x.c_class == 'basic' else
            (self.ranks[self.orders[w := type_to_idx[x.c_type]][0]][w] + self.ranks[self.orders[w][1]][w]),
            self.hand_freq[type_to_idx[x.c_type]])
        ))
        
        self.last = self.hand[-1].c_type
        self.hand_freq[type_to_idx[self.last]] -= 1

        if self.hand[-1].c_class == 'bouncy':
            order1 = self.orders[type_to_idx[self.last]][0]
            order2 = self.orders[type_to_idx[self.last]][1]
            return self.hand.pop(), (order1, order2), (self.skill_check(), self.skill_check())
        else:
            order = self.orders[type_to_idx[self.last]][0]
            return self.hand.pop(), (order,), (self.skill_check(),)
    
    def backward(self, cards: list[tuple[str, int]]):
        old_prob = [
            (2 - min(2, self.type_freq[2])) / (6 - self.count),
            (2 - min(2, self.type_freq[0])) / (6 - self.count),
            (2 - min(2, self.type_freq[1])) / (6 - self.count),
        ]
        for card, idx in cards:
            if card is not None:
                self.type_freq[type_to_idx[card]] += 1
                if not self.flipped[idx]:
                    self.count += 1
                self.flipped[idx] = True
        
        if self.count == 6: return
        
        new_prob = [
            (2 - min(2, self.type_freq[2])) / (6 - self.count),
            (2 - min(2, self.type_freq[0])) / (6 - self.count),
            (2 - min(2, self.type_freq[1])) / (6 - self.count),
        ]

        self.ranks = [[(row[x] / old_prob[x] * new_prob[x] if old_prob[x] != 0 else 0) for x in range(3)] for row in self.ranks]

        for card, idx in cards:
            if card is not None:
                self.ranks[idx] = [-1e5, -1e5, -1e5]
            elif self.skill_passed:
                self.ranks[idx] = [a * b for a, b in zip(self.ranks[idx], self.rank_mod[type_to_idx[self.last]])]

        for i, row in enumerate(self.ranks):
            whole = sum(row)
            if whole == 0:
                self.ranks[i] = [-1e5, -1e5, -1e5]
            else:
                self.ranks[i] = [x / whole for x in row]

class Game:
    def __init__(self, agent1: Agent, agent2: Agent, verbose=True):
        self.agent1 = agent1
        self.table1 = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ]
        random.shuffle(self.table1)
        self.shown1 = [False] * num_cards

        self.agent2 = agent2
        self.table2 = [
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic'),
            Card(card_type='light', card_class='basic'),
            Card(card_type='regular', card_class='basic'),
            Card(card_type='heavy', card_class='basic')
        ]
        random.shuffle(self.table2)
        self.shown2 = [False] * num_cards

        self.flip_chance = [
            [1, 0, 2],
            [2, 1, 0],
            [0, 2, 1]
        ]
        self.verbose = verbose

        self.count1 = 0
        self.count2 = 0

        for agent, table in zip([self.agent1, self.agent2], [self.table1, self.table2]):
            if agent.omamori == 'vision':
                idx = random.sample(range(6), 2)
                card = table[idx[0]]
                agent.show(card.c_type, idx[0])
                card = table[idx[1]]
                agent.show(card.c_type, idx[1])

        if self.verbose:
            print('Agent 1:')
            print(draw_cards(self.table1, self.shown1))
            print('Agent 2:')
            print(draw_cards(self.table2, self.shown2))

    def update1(self, idx):
        self.shown1[idx] = True
        self.count1 += 1
    
    def update2(self, idx):
        self.shown2[idx] = True
        self.count2 += 1

    def turn(self, agent: Agent, table: list[Card], update, name, other_agent: Agent):
        if agent.omamori == 'trade':
            if len(other_agent.hand) > 0:
                idx = random.randint(0, len(other_agent.hand)-1)
                card = agent.trade(other_agent.hand[idx])
                if self.verbose:
                    print(f"Traded Agent {name}'s {str(card)} for {str(other_agent.hand[idx])}")
                other_agent.hand[idx] = card
        card, table_idx, check = agent.move()
        
        result = []
        for i, c in zip(table_idx, check):
            threshold = type_prob[self.flip_chance[type_to_idx[card.c_type]][type_to_idx[table[i].c_type]]]
            if random.random() < threshold * c:
                if self.verbose:
                    print(f'Agent {name} threw {str(card)} at {i} and it flipped')
                update(i)
                result.append((table[i].c_type, i))
            else:
                if self.verbose:
                    print(f'Agent {name} threw {str(card)} at {i} and it did not flip')
                result.append((None, i))
            if card.c_class == 'ceramic':
                for j in [x for x in [i-1, i+1] if 0 <= x <= 5]:
                    if random.random() < threshold * c * ceramic_prob:
                        if self.verbose:
                            print(f'Ceramic card hit {str(card)} at {j} and it flipped')
                        update(j)
                        result.append((table[j].c_type, j))
                    else:
                        if self.verbose:
                            print(f'Ceramic card hit {str(card)} at {j} and it did not flip')
                        result.append((None, j))

        agent.backward(result)

    def round(self):
        self.turn(self.agent1, self.table1, self.update1, 1, self.agent2)
        self.turn(self.agent2, self.table2, self.update2, 2, self.agent1)
        
        if self.verbose:
            print('Agent 1:')
            print(draw_cards(self.table1, self.shown1))
            print('Agent 2:')
            print(draw_cards(self.table2, self.shown2))

        return sum(self.shown1) == num_cards or sum(self.shown2) == num_cards

    def winner(self):
        if self.count1 > self.count2:
            return 1
        elif self.count1 < self.count2:
            return 2
        else:
            play1, play2 = 0, 0
            threshold = 0.5
            while play1 == play2:
                play1 = self.agent1.skill_check(threshold)
                play2 = self.agent2.skill_check(threshold)
                if threshold > 0.05:
                    threshold * 0.9

            return int(play1 < play2) + 1
        
    def run(self):
        for i in range(9):
            if self.verbose:
                print(f'\n\nRound {i+1}!!')
            if self.round(): break

        return self.winner()

verbose = False
agents = [Agent0, Agent1, Agent2, Agent3, Agent4, Agent5, Agent6]
for i in range(len(agents)-1):
    results = []
    for _ in range(30000):
        human = Human(**presets['default'])
        game = Game(agents[i](), agents[i+1](), verbose=verbose)
        results.append(game.run())

    counter = Counter(results)
    print(counter)
    if len(results) > 0:
        print(counter[2] / (counter[1] + counter[2]))

# results = []
# verbose = False
# for i in range(10000):
#     human = Human(**presets['default'])
#     game = Game(Agent0(), Agent1(), verbose=verbose)
#     results.append(game.run())

# counter = Counter(results)
# print(counter)
# if len(results) > 0:
#     print(counter[2] / (counter[1] + counter[2]))